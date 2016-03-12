using System;
using System.Windows.Forms;

namespace LyncBlinkBridge
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void buttonAboutOK_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
