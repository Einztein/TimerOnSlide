using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

using Office = Microsoft.Office.Core;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;

namespace P16TimerOnSlide
{
    public enum TimerMode
    {
        CountUp,
        CountDown
    }

    public sealed class TimerOverlayController : IDisposable
    {
        private readonly PowerPoint.Application _app;
        private readonly Timer _uiTimer;
        private readonly Stopwatch _stopwatch;
        private HotKeyWindow _hotKeyWindow;

        private TimerMode _mode = TimerMode.CountUp;
        private TimeSpan _countDownStart = TimeSpan.FromMinutes(5);
        private TimeSpan _accumulated = TimeSpan.Zero;

        private bool _isRunning;
        private bool _alarmFired;

        public const string OverlayShapeName = "__VSTO_TIMER_OVERLAY__";

        private const int HK_START_PAUSE = 1001;
        private const int HK_RESET = 1002;
        private const int HK_COUNTUP = 1003;
        private const int HK_COUNTDOWN = 1004;
        private const int HK_INSERT = 1005;

        public TimerOverlayController(PowerPoint.Application app)
        {
            _app = app;
            _stopwatch = new Stopwatch();

            _uiTimer = new Timer
            {
                Interval = 15
            };
            _uiTimer.Tick += (_, __) => UpdateOverlay();
        }

        public string CountdownSourceText => FormatTime(_countDownStart, false);

        public void Initialize()
        {
            _hotKeyWindow = new HotKeyWindow();
            _hotKeyWindow.HotKeyPressed += OnHotKeyPressed;

            uint mods = (uint)(HotKeyModifiers.Control | HotKeyModifiers.Alt | HotKeyModifiers.Shift | HotKeyModifiers.NoRepeat);

            List<string> failed = new List<string>();

            if (!_hotKeyWindow.TryRegister(HK_START_PAUSE, Keys.S, mods))
                failed.Add("Ctrl+Alt+Shift+S  TimerStartStop");

            if (!_hotKeyWindow.TryRegister(HK_RESET, Keys.R, mods))
                failed.Add("Ctrl+Alt+Shift+R  TimerReset");

            if (!_hotKeyWindow.TryRegister(HK_COUNTUP, Keys.U, mods))
                failed.Add("Ctrl+Alt+Shift+U  TimerStopwatch");

            if (!_hotKeyWindow.TryRegister(HK_COUNTDOWN, Keys.D, mods))
                failed.Add("Ctrl+Alt+Shift+D  TimerCountdown");

            if (!_hotKeyWindow.TryRegister(HK_INSERT, Keys.I, mods))
                failed.Add("Ctrl+Alt+Shift+I  TimerInserShow");

            if (failed.Count > 0)
            {
                MessageBox.Show(
                    "Shortcut Register Failed on ：\r\n\r\n" +
                    string.Join("\r\n", failed),
                    "PPT Timer",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            _uiTimer.Start();
        }

        public void Dispose()
        {
            _uiTimer.Stop();
            _uiTimer.Dispose();

            if (_hotKeyWindow != null)
            {
                _hotKeyWindow.HotKeyPressed -= OnHotKeyPressed;
                _hotKeyWindow.Dispose();
                _hotKeyWindow = null;
            }
        }

        private void OnHotKeyPressed(int id)
        {
            switch (id)
            {
                case HK_START_PAUSE:
                    StartOrPause();
                    break;
                case HK_RESET:
                    Reset();
                    break;
                case HK_COUNTUP:
                    SwitchToCountUpMode();
                    break;
                case HK_COUNTDOWN:
                    SwitchToCountDownMode();
                    break;
                case HK_INSERT:
                    EnsureOverlayOnActiveSlide();
                    break;
            }
        }

        public void EnsureOverlayOnActiveSlide()
        {
            PowerPoint.Slide slide = GetActiveSlide();
            if (slide == null) return;

            PowerPoint.Shape shape = EnsureOverlayOnSlide(slide);
            shape.ZOrder(Office.MsoZOrderCmd.msoBringToFront);
            UpdateOverlay();
        }

        public void StartOrPause()
        {
            if (_isRunning)
            {
                _accumulated += _stopwatch.Elapsed;
                _stopwatch.Reset();
                _isRunning = false;
            }
            else
            {
                _stopwatch.Restart();
                _isRunning = true;
            }

            UpdateOverlay();
        }

        public void Reset()
        {
            _isRunning = false;
            _stopwatch.Reset();
            _accumulated = TimeSpan.Zero;
            _alarmFired = false;
            UpdateOverlay();
        }

        public void SwitchToCountUpMode()
        {
            _mode = TimerMode.CountUp;
            Reset();
        }

        public void SwitchToCountDownMode()
        {
            _mode = TimerMode.CountDown;
            Reset();
        }

        public bool TrySetCountDown(string text, out string error)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                error = "Input countdown time such as 00:05:00.000 or 05:00";
                return false;
            }

            if (!TryParseTimeText(text.Trim(), out TimeSpan value))
            {
                error = "Time should be formatted as：hh:mm:ss.fff、hh:mm:ss、mm:ss.fff、mm:ss";
                return false;
            }

            if (value < TimeSpan.Zero)
            {
                error = "Cannot be negative number";
                return false;
            }

            _countDownStart = value;
            _alarmFired = false;
            UpdateOverlay();

            error = null;
            return true;
        }

        private void UpdateOverlay()
        {
            PowerPoint.Slide slide = GetActiveSlide();
            if (slide == null) return;

            PowerPoint.Shape shape = FindOverlayShape(slide);
            if (shape == null) return;

            string text = BuildDisplayText(out bool negative);

            if (_mode == TimerMode.CountDown && negative && !_alarmFired)
            {
                _alarmFired = true;
                PlayAlarm();
            }

            ApplyText(shape, text, negative);
        }

        private string BuildDisplayText(out bool negative)
        {
            TimeSpan progressed = _accumulated + (_isRunning ? _stopwatch.Elapsed : TimeSpan.Zero);

            if (_mode == TimerMode.CountUp)
            {
                negative = false;
                return FormatTime(progressed, false);
            }

            TimeSpan remain = _countDownStart - progressed;

            if (remain >= TimeSpan.Zero)
            {
                negative = false;
                return FormatTime(remain, false);
            }

            negative = true;
            return FormatTime(remain.Duration(), true);
        }

        private void PlayAlarm()
        {
            SystemSounds.Exclamation.Play();
        }

        private PowerPoint.Slide GetActiveSlide()
        {
            try
            {
                if (_app.SlideShowWindows != null && _app.SlideShowWindows.Count > 0)
                {
                    return _app.SlideShowWindows[1].View.Slide as PowerPoint.Slide;
                }
            }
            catch
            {
            }

            try
            {
                var win = _app.ActiveWindow;
                if (win != null && win.View != null && win.View.Slide != null)
                {
                    return win.View.Slide;
                }
            }
            catch
            {
            }

            try
            {
                var sel = _app.ActiveWindow?.Selection;
                if (sel != null && sel.SlideRange != null && sel.SlideRange.Count > 0)
                {
                    return sel.SlideRange[1];
                }
            }
            catch
            {
            }

            return null;
        }

        private PowerPoint.Shape FindOverlayShape(PowerPoint.Slide slide)
        {
            try
            {
                for (int i = 1; i <= slide.Shapes.Count; i++)
                {
                    PowerPoint.Shape s = slide.Shapes[i];
                    if (s.Name == OverlayShapeName)
                        return s;
                }
            }
            catch
            {
            }

            return null;
        }

        private PowerPoint.Shape EnsureOverlayOnSlide(PowerPoint.Slide slide)
        {
            PowerPoint.Shape shape = FindOverlayShape(slide);
            if (shape != null) return shape;

            shape = slide.Shapes.AddTextbox(
                Office.MsoTextOrientation.msoTextOrientationHorizontal,
                20f, 20f, 280f, 50f);

            shape.Name = OverlayShapeName;
            shape.Fill.Visible = Office.MsoTriState.msoTrue;
            shape.Fill.ForeColor.RGB = ColorTranslator.ToOle(Color.Black);
            shape.Fill.Transparency = 0.25f;

            shape.Line.Visible = Office.MsoTriState.msoFalse;

            shape.TextFrame.MarginLeft = 6f;
            shape.TextFrame.MarginRight = 6f;
            shape.TextFrame.MarginTop = 2f;
            shape.TextFrame.MarginBottom = 2f;

            shape.TextFrame.TextRange.Text = "00:00:00.000";
            //shape.TextFrame.TextRange.Font.Name = "Consolas";
            //shape.TextFrame.TextRange.Font.Size = 28;
            //shape.TextFrame.TextRange.Font.Bold = Office.MsoTriState.msoTrue;
            shape.TextFrame.TextRange.ParagraphFormat.Alignment = PowerPoint.PpParagraphAlignment.ppAlignCenter;
            shape.ZOrder(Office.MsoZOrderCmd.msoBringToFront);

            ApplyText(shape, BuildDisplayText(out bool negative), negative);

            return shape;
        }

        private void ApplyText(PowerPoint.Shape shape, string text, bool negative)
        {
            shape.TextFrame.TextRange.Text = text;
            shape.TextFrame.TextRange.Font.Color.RGB =
                ColorTranslator.ToOle(negative ? Color.Red : Color.White);
        }

        private static string FormatTime(TimeSpan ts, bool negative)
        {
            long totalHours = (long)Math.Floor(ts.TotalHours);
            string sign = negative ? "-" : "";
            return $"{sign}{totalHours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds:000}";
        }

        private static bool TryParseTimeText(string text, out TimeSpan value)
        {
            value = TimeSpan.Zero;

            string[] parts = text.Split(':');
            if (parts.Length != 2 && parts.Length != 3)
                return false;

            int h = 0, m = 0, s = 0, ms = 0;

            if (parts.Length == 2)
            {
                if (!int.TryParse(parts[0], out m)) return false;
                if (!TryParseSecondPart(parts[1], out s, out ms)) return false;
            }
            else
            {
                if (!int.TryParse(parts[0], out h)) return false;
                if (!int.TryParse(parts[1], out m)) return false;
                if (!TryParseSecondPart(parts[2], out s, out ms)) return false;
            }

            if (h < 0 || m < 0 || s < 0 || ms < 0) return false;
            if (m > 59 || s > 59 || ms > 999) return false;

            value =
                TimeSpan.FromHours(h) +
                TimeSpan.FromMinutes(m) +
                TimeSpan.FromSeconds(s) +
                TimeSpan.FromMilliseconds(ms);

            return true;
        }

        private static bool TryParseSecondPart(string input, out int seconds, out int milliseconds)
        {
            seconds = 0;
            milliseconds = 0;

            string[] parts = input.Split('.');
            if (parts.Length < 1 || parts.Length > 2)
                return false;

            if (!int.TryParse(parts[0], out seconds))
                return false;

            if (parts.Length == 2)
            {
                string msText = parts[1];
                if (msText.Length > 3) return false;
                if (!int.TryParse(msText, out milliseconds)) return false;

                if (msText.Length == 1) milliseconds *= 100;
                else if (msText.Length == 2) milliseconds *= 10;
            }

            return true;
        }
    }

}
