using System.Windows.Forms;

namespace P16TimerOnSlide
{
    public sealed class TimerConfigForm : Form
    {
        private Label label1;
        private TextBox textBoxTime;
        private Button btnOK;
        private Button btnBye;

        public string CountdownText => textBoxTime.Text.Trim();

        public TimerConfigForm(string currentValue)
        {
            InitializeComponent();
            textBoxTime.Text = currentValue;
        }

        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTime = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnBye = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(365, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Format：hh:mm:ss.fff or mm:ss，such as 00:05:00.000 or 05:00";
            // 
            // textBoxTime
            // 
            this.textBoxTime.Location = new System.Drawing.Point(12, 24);
            this.textBoxTime.Name = "textBoxTime";
            this.textBoxTime.Size = new System.Drawing.Size(365, 21);
            this.textBoxTime.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(245, 62);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "Confirm";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnBye
            // 
            this.btnBye.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBye.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnBye.Location = new System.Drawing.Point(326, 62);
            this.btnBye.Name = "btnBye";
            this.btnBye.Size = new System.Drawing.Size(75, 23);
            this.btnBye.TabIndex = 3;
            this.btnBye.Text = "Cancel";
            this.btnBye.UseVisualStyleBackColor = true;
            // 
            // TimerConfigForm
            // 
            this.AcceptButton = this.btnOK;
            this.CancelButton = this.btnBye;
            this.ClientSize = new System.Drawing.Size(413, 97);
            this.Controls.Add(this.btnBye);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.textBoxTime);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(429, 136);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(429, 136);
            this.Name = "TimerConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Timer Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

    }

}
