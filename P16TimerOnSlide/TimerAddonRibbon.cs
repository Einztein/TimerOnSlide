using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using Office = Microsoft.Office.Core;

namespace P16TimerOnSlide
{
    [ComVisible(true)]
    public class TimerAddonRibbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;

        public TimerAddonRibbon()
        {
        }

        #region IRibbonExtensibility Members

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("P16TimerOnSlide.TimerAddonRibbon.xml");
        }

        #endregion

        #region Ribbon Callbacks
        //Create callback methods here. For more information about adding callback methods, visit https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            ribbon = ribbonUI;
        }

        public void OnInsertOverlay(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.Controller.EnsureOverlayOnActiveSlide();
        }

        public void OnSetCountdown(Office.IRibbonControl control)
        {
            using (var dlg = new TimerConfigForm(Globals.ThisAddIn.Controller.CountdownSourceText))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    if (!Globals.ThisAddIn.Controller.TrySetCountDown(dlg.CountdownText, out string error))
                    {
                        MessageBox.Show(error, "PPT Timer", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    Globals.ThisAddIn.Controller.SwitchToCountDownMode();
                }
            }
        }

        public void OnCountUp(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.Controller.SwitchToCountUpMode();
        }

        public void OnCountDown(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.Controller.SwitchToCountDownMode();
        }

        public void OnStartPause(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.Controller.StartOrPause();
        }

        public void OnReset(Office.IRibbonControl control)
        {
            Globals.ThisAddIn.Controller.Reset();
        }

        #endregion

        #region Helpers

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
