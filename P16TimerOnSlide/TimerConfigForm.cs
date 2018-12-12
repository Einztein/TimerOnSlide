using System.Windows.Forms;

namespace P16TimerOnSlide
{
    public sealed class TimerConfigForm : Form
    {
        private readonly TextBox _txt;

        public string CountdownText => _txt.Text.Trim();

        public TimerConfigForm(string currentValue)
        {
            Text = "Set Countdown";
            Width = 430;
            Height = 180;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterScreen;
            MaximizeBox = false;
            MinimizeBox = false;

            var lbl = new Label
            {
                Left = 20,
                Top = 20,
                Width = 370,
                Text = "Format：hh:mm:ss.fff or mm:ss，such as 00:05:00.000 或 05:00"
            };

            _txt = new TextBox
            {
                Left = 20,
                Top = 50,
                Width = 370,
                Text = currentValue
            };

            var btnOk = new Button
            {
                Text = "Confirm",
                Left = 230,
                Top = 90,
                Width = 75,
                DialogResult = DialogResult.OK
            };

            var btnCancel = new Button
            {
                Text = "Cancel",
                Left = 315,
                Top = 90,
                Width = 75,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(lbl);
            Controls.Add(_txt);
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            AcceptButton = btnOk;
            CancelButton = btnCancel;
        }
    }

}
