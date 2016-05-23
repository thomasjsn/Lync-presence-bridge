
using System;
using System.Windows.Forms;

using Microsoft.Lync.Model;

using ThingM.Blink1;
using ThingM.Blink1.ColorProcessor;
using System.Management;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using Uctrl.Arduino;

namespace LyncPresenceBridge
{
    class LyncConnectorAppContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayIconContextMenu;

        private LyncClient lyncClient;
        private Blink1 blink1 = new Blink1();

        private Arduino arduino = new Arduino();

        private ManagementEventWatcher usbWatcher;

        private bool isLyncIntegratedMode = true;

        private Rgb colorAvailable = new Rgb(0, 150, 17);
        private Rgb colorBusy = new Rgb(150, 0, 0);
        private Rgb colorAway = new Rgb(150, 150, 0);
        private Rgb colorOff = new Rgb(0, 0, 0);

        private byte[] ledsAvailableArduino = { 255, 0, 0 };
        private byte[] ledsAvailableIdleArduino = { 255, 255, 0 };
        private byte[] ledsBusyArduino = { 0, 0, 255 };
        private byte[] ledsBusyIdleArduino = { 0, 255, 255 };
        private byte[] ledsAwayArduino = { 0, 50, 0 };
        private byte[] ledsOffArduino = { 0, 0, 0 };


        public LyncConnectorAppContext()
        {
            Application.ApplicationExit += new System.EventHandler(this.OnApplicationExit);

            // Setup UI, NotifyIcon
            InitializeComponent();

            trayIcon.Visible = true;

            // Setup Blink
            InitializeBlink1();

            // Setup port
            if (! arduino.OpenPort("COM" + Properties.Settings.Default.ArduinoSerialPort.ToString()))
            {
                trayIcon.ShowBalloonTip(1000, "Error", "Could not open and init serial port.", ToolTipIcon.Warning);
            }

            // Setup Lync Client Connection
            GetLyncClient();

            // Watch for USB Changes, try to monitor blink plugin/removal
            InitializeUSBWatcher();

        }

        private bool InitializeBlink1()
        {
            try
            {
                blink1.Open();
            }
            catch (InvalidOperationException iox)
            {
                // No blink devices attached, switching to loacl mode (in the future) 
                Debug.WriteLine(iox.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }

            return blink1.IsConnected;

        }

        private void InitializeComponent()
        {
            trayIcon = new NotifyIcon();

            //The icon is added to the project resources.
            trayIcon.Icon = Properties.Resources.blink_off;

            // TrayIconContextMenu
            trayIconContextMenu = new ContextMenuStrip();
            trayIconContextMenu.SuspendLayout();
            trayIconContextMenu.Name = "TrayIconContextMenu";

            // Tray Context Menuitems to set color
            this.trayIconContextMenu.Items.Add("Available", null, new EventHandler(AvailableMenuItem_Click));
            this.trayIconContextMenu.Items.Add("Busy", null, new EventHandler(BusyMenuItem_Click));
            this.trayIconContextMenu.Items.Add("Away", null, new EventHandler(AwayMenuItem_Click));
            this.trayIconContextMenu.Items.Add("Off", null, new EventHandler(OffMenuItem_Click));

            // Separation Line
            this.trayIconContextMenu.Items.Add(new ToolStripSeparator());

            // About Form Line
            this.trayIconContextMenu.Items.Add("About", null, new EventHandler(aboutMenuItem_Click));

            // Settings Form Line
            this.trayIconContextMenu.Items.Add("Settings", null, new EventHandler(settingsMenuItem_Click));

            // Separation Line
            this.trayIconContextMenu.Items.Add(new ToolStripSeparator());

            // CloseMenuItem
            this.trayIconContextMenu.Items.Add("Exit", null, new EventHandler(CloseMenuItem_Click));


            trayIconContextMenu.ResumeLayout(false);
            trayIcon.ContextMenuStrip = trayIconContextMenu;
        }

        private void GetLyncClient()
        {
            try
            {
                // try to get the running lync client and register for change events, if Client is not running then ClientNoFound Exception is thrown by lync api
                lyncClient = LyncClient.GetClient();
                lyncClient.StateChanged += lyncClient_StateChanged;
                
                if (lyncClient.State == ClientState.SignedIn)
                    lyncClient.Self.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
    
                SetCurrentContactState();
            }
            catch (ClientNotFoundException)
            {
                Debug.WriteLine("Lync Client not started.");

                SetLyncIntegrationMode(false);

                trayIcon.ShowBalloonTip(1000, "Error", "Lync Client not started. Running in manual mode now. Please use the context menu to change your blink color.", ToolTipIcon.Warning);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());

                trayIcon.ShowBalloonTip(1000, "Error", "Something went wrong by getting your Lync status. Running in manual mode now. Please use the context menu to change your blink color.", ToolTipIcon.Warning);
                Debug.WriteLine(e.Message);
            }
        }

        void SetLyncIntegrationMode(bool isLyncIntegrated)
        {
            isLyncIntegratedMode = isLyncIntegrated;
            if (isLyncIntegratedMode)
            {
            }
            else
            {
            }
        }

        /// <summary>
        /// Read the current Availability Information from Lync/Skype for Business and set the color 
        /// </summary>
        void SetCurrentContactState()
        {
            Rgb blinkColor = colorOff;
            byte[] arduinoLeds = ledsOffArduino;

            if (lyncClient.State == ClientState.SignedIn)
            {
                ContactAvailability currentAvailability = (ContactAvailability)lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
                switch (currentAvailability)
                {
                    case ContactAvailability.Busy:              // Busy
                        blinkColor = colorBusy;
                        arduinoLeds = ledsBusyArduino;
                        break;

                    case ContactAvailability.BusyIdle:          // Busy and idle
                        blinkColor = colorBusy;
                        arduinoLeds = ledsBusyIdleArduino;
                        break;

                    case ContactAvailability.Free:              // Available
                        blinkColor = colorAvailable;
                        arduinoLeds = ledsAvailableArduino;
                        break;

                    case ContactAvailability.FreeIdle:          // Available and idle
                        blinkColor = colorAvailable;
                        arduinoLeds = ledsAvailableIdleArduino;
                        break;

                    case ContactAvailability.Away:              // Inactive/away, off work, appear away
                    case ContactAvailability.TemporarilyAway:   // Be right back
                        blinkColor = colorAway;
                        arduinoLeds = ledsAwayArduino;
                        break;

                    case ContactAvailability.DoNotDisturb:      // Do not disturb
                        blinkColor = colorBusy;
                        arduinoLeds = ledsBusyArduino;
                        break;

                    case ContactAvailability.Offline:           // Offline
                        blinkColor = colorOff;
                        arduinoLeds = ledsOffArduino;
                        break;

                    default:
                        break;
                }

                SetBlink1State(blinkColor);
                arduino.SetLEDs(arduinoLeds);

                Debug.WriteLine(currentAvailability.ToString());
            }
        }

        void SetBlink1State(Rgb color)
        {
            bool setColorResult = false;

            if (blink1.IsConnected)
            {
                setColorResult = blink1.SetColor(color);
                if (setColorResult)
                {
                    Debug.WriteLine("Successful set blink1 to {0},{1},{2}", color.Red, color.Green, color.Blue);
                }
                else
                {
                    Debug.WriteLine("Error setting blink1 to {0},{1},{2}", color.Red, color.Green, color.Blue);
                }
            }

            SetIconState(color);

        }

        void SetIconState(Rgb color)
        {
                
            using (Bitmap b = Bitmap.FromHicon(new Icon( Properties.Resources.blink_off , 48, 48).Handle))
            {
                if (color.Blue == 0 && color.Green == 0 && color.Red == 0)
                {
                    // if black , then we do not modify the image. We may need a picture unavailable build here.
                }
                else
                {
                    Graphics g = Graphics.FromImage(b);
                    g.FillRegion(new SolidBrush(Color.FromArgb(color.Red, color.Green, color.Blue)), new Region(new Rectangle(20, 29, 22, 27)));

                }

                IntPtr Hicon = b.GetHicon();
                Icon newIcon = Icon.FromHandle(Hicon);
                trayIcon.Icon = newIcon;
            }
        }

        void lyncClient_StateChanged(object sender, ClientStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ClientState.Initializing:
                    break;

                case ClientState.Invalid:
                    break;

                case ClientState.ShuttingDown:
                    break;

                case ClientState.SignedIn:
                    lyncClient.Self.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
                    SetCurrentContactState();
                    break;

                case ClientState.SignedOut:
                    trayIcon.ShowBalloonTip(1000, "", "You signed out in Lync. Switching to manual mode.", ToolTipIcon.Info);
                    break;

                case ClientState.SigningIn:
                    break;

                case ClientState.SigningOut:
                    break;

                case ClientState.Uninitialized:
                    break;

                default:
                    break;
            }
        }

        void Contact_ContactInformationChanged(object sender, ContactInformationChangedEventArgs e)
        {
            if (e.ChangedContactInformation.Contains(ContactInformationType.Availability))
            {
                SetCurrentContactState();
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            trayIcon.Visible = false;

            // stop (USB) ManagementEventWatcher
            usbWatcher.Stop();
            usbWatcher.Dispose();

            // Close blink Connection and switch off LED
            if (blink1.IsConnected)
            {
                blink1.Close();
            }

            if (arduino.Port.IsOpen)
            {
                arduino.SetLEDs(ledsOffArduino);
                arduino.Dispose();
            }
                
        }
        
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void aboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm about = new AboutForm();
            about.ShowDialog();
        }

        private void settingsMenuItem_Click(object sender, EventArgs e)
        {
            SettingsForm settings = new SettingsForm();
            settings.ShowDialog();
        }

        private void OffMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorOff);
            arduino.SetLEDs(ledsOffArduino);
        }

        private void AwayMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorAway);
            arduino.SetLEDs(ledsAwayArduino);
        }

        private void BusyMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorBusy);
            arduino.SetLEDs(ledsBusyArduino);
        }

        private void AvailableMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorAvailable);
            arduino.SetLEDs(ledsAvailableArduino);
        }

        // Watch for USB changes to detect blink(1) removal
        private void InitializeUSBWatcher()
        {
            usbWatcher = new ManagementEventWatcher();
            var query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");
            usbWatcher.EventArrived += new EventArrivedEventHandler(watcher_EventArrived);
            usbWatcher.Query = query;
            usbWatcher.Start();
        }

        private void watcher_EventArrived(object sender, EventArrivedEventArgs e)
        {
            // Check if blink was removed
            // do not know what we do then, some hint to user?
            InitializeBlink1();

            if (blink1.IsConnected)
            {
                Debug.WriteLineIf(blink1.IsConnected, "USB change, Blink(1) available");

                // timing problem in blink(1) if we set the state to fast after plugin change, wait 100ms
                Thread.Sleep(100);
                SetCurrentContactState();
            }
            else
            {
                Debug.WriteLineIf(!blink1.IsConnected, "USB change, Blink(1) not available");
            }
        }
    }
}
