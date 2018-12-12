using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using PowerPoint = Microsoft.Office.Interop.PowerPoint;
using Office = Microsoft.Office.Core;


namespace P16TimerOnSlide
{
    public partial class ThisAddIn
    {
        public TimerOverlayController Controller { get; private set; }
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            Controller = new TimerOverlayController(this.Application);
            Controller.Initialize();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            Controller?.Dispose();
        }

        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            return new TimerAddonRibbon();
        }

        #region VSTO 生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        #endregion
    }
}
