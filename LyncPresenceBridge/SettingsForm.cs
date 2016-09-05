using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LyncPresenceBridge
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.ArduinoSerialPort = (int)numSerialPort.Value;

            byte[] colorAvailable = { (byte)numColorAvailable1.Value, (byte)numColorAvailable2.Value, (byte)numColorAvailable3.Value };
            Properties.Settings.Default.ColorAvailable = string.Join(",", colorAvailable);

            byte[] colorAvailableIdle = { (byte)numColorAvailableIdle1.Value, (byte)numColorAvailableIdle2.Value, (byte)numColorAvailableIdle3.Value };
            Properties.Settings.Default.ColorAvailableIdle = string.Join(",", colorAvailableIdle);

            byte[] colorBusy = { (byte)numColorBusy1.Value, (byte)numColorBusy2.Value, (byte)numColorBusy3.Value };
            Properties.Settings.Default.ColorBusy = string.Join(",", colorBusy);

            byte[] colorBusyIdle = { (byte)numColorBusyIdle1.Value, (byte)numColorBusyIdle2.Value, (byte)numColorBusyIdle3.Value };
            Properties.Settings.Default.ColorBusyIdle = string.Join(",", colorBusyIdle);

            byte[] colorAway = { (byte)numColorAway1.Value, (byte)numColorAway2.Value, (byte)numColorAway3.Value };
            Properties.Settings.Default.ColorAway = string.Join(",", colorAway);

            byte[] colorOff = { (byte)numColorOff1.Value, (byte)numColorOff2.Value, (byte)numColorOff3.Value };
            Properties.Settings.Default.ColorOff = string.Join(",", colorOff);

            Properties.Settings.Default.Save();
            this.Close();
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            numSerialPort.Value = Properties.Settings.Default.ArduinoSerialPort;

            byte[] colorAvailable = Array.ConvertAll(Properties.Settings.Default.ColorAvailable.Split(','), s => Convert.ToByte(s));
            numColorAvailable1.Value = colorAvailable[0];
            numColorAvailable2.Value = colorAvailable[1];
            numColorAvailable3.Value = colorAvailable[2];

            byte[] colorAvailableIdle = Array.ConvertAll(Properties.Settings.Default.ColorAvailableIdle.Split(','), s => Convert.ToByte(s));
            numColorAvailableIdle1.Value = colorAvailableIdle[0];
            numColorAvailableIdle2.Value = colorAvailableIdle[1];
            numColorAvailableIdle3.Value = colorAvailableIdle[2];

            byte[] colorBusy = Array.ConvertAll(Properties.Settings.Default.ColorBusy.Split(','), s => Convert.ToByte(s));
            numColorBusy1.Value = colorBusy[0];
            numColorBusy2.Value = colorBusy[1];
            numColorBusy3.Value = colorBusy[2];

            byte[] colorBusyIdle = Array.ConvertAll(Properties.Settings.Default.ColorBusyIdle.Split(','), s => Convert.ToByte(s));
            numColorBusyIdle1.Value = colorBusyIdle[0];
            numColorBusyIdle2.Value = colorBusyIdle[1];
            numColorBusyIdle3.Value = colorBusyIdle[2];

            byte[] colorAway = Array.ConvertAll(Properties.Settings.Default.ColorAway.Split(','), s => Convert.ToByte(s));
            numColorAway1.Value = colorAway[0];
            numColorAway2.Value = colorAway[1];
            numColorAway3.Value = colorAway[2];

            byte[] colorOff = Array.ConvertAll(Properties.Settings.Default.ColorOff.Split(','), s => Convert.ToByte(s));
            numColorOff1.Value = colorOff[0];
            numColorOff2.Value = colorOff[1];
            numColorOff3.Value = colorOff[2];
        }
    }
}
