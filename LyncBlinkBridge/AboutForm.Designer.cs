namespace LyncBlinkBridge
{
    partial class AboutForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonAboutOK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonAboutOK
            // 
            this.buttonAboutOK.Location = new System.Drawing.Point(217, 69);
            this.buttonAboutOK.Name = "buttonAboutOK";
            this.buttonAboutOK.Size = new System.Drawing.Size(75, 23);
            this.buttonAboutOK.TabIndex = 0;
            this.buttonAboutOK.Text = "OK";
            this.buttonAboutOK.UseVisualStyleBackColor = true;
            this.buttonAboutOK.Click += new System.EventHandler(this.buttonAboutOK_Click);
            // 
            // AboutForm
            // 
            this.AcceptButton = this.buttonAboutOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(304, 104);
            this.Controls.Add(this.buttonAboutOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "About LyncBlinker";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonAboutOK;
    }
}