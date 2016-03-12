
using System;
using System.Windows.Forms;

using Microsoft.Lync.Model;

using ThingM.Blink1;
using ThingM.Blink1.ColorProcessor;

namespace LyncBlinkBridge
{
    class BlinkLyncConnectorAppContext : ApplicationContext
    {
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayIconContextMenu;
        private ToolStripMenuItem closeMenuItem;
        private ToolStripMenuItem aboutMenuItem;
        private LyncClient lyncClient;
        private Blink1 blink1 = new Blink1();
        private bool isLyncIntegratedMode = true;

        private Rgb colorAvailable = new Rgb(0, 150, 17);
        private Rgb colorBusy = new Rgb(150, 0, 0);
        private Rgb colorAway = new Rgb(150, 150, 0);
        private Rgb colorOff = new Rgb(0, 0, 0);


        public BlinkLyncConnectorAppContext()
        {
            Application.ApplicationExit += new System.EventHandler(this.OnApplicationExit);

            try
            { 
                blink1.Open();
            }
            catch (InvalidOperationException iox)
            {
                Console.Write(iox.ToString());
            }
            catch (Exception e)
            {
                Console.Write(e.ToString());
            }

            InitializeComponent();
            trayIcon.Visible = true;

            //
            // Lync Client Connection
            //
            GetLyncClient();

        }

        private void InitializeComponent()
        {
            trayIcon = new NotifyIcon();

            //The icon is added to the project resources.
            //Here I assume that the name of the file is 'TrayIcon.ico'
            trayIcon.Icon = Properties.Resources.TrayIcon;

            //handle doubleclicks on the icon:
            trayIcon.DoubleClick += TrayIcon_DoubleClick;

            // 
            // TrayIconContextMenu
            // 
            trayIconContextMenu = new ContextMenuStrip();
            trayIconContextMenu.SuspendLayout();
            trayIconContextMenu.Name = "TrayIconContextMenu";

            this.trayIconContextMenu.Items.Add("Available", null, new EventHandler(AvailableMenuItem_Click));
            this.trayIconContextMenu.Items.Add("Busy", null, new EventHandler(BusyMenuItem_Click));
            this.trayIconContextMenu.Items.Add("Away", null, new EventHandler(AwayMenuItem_Click));
            this.trayIconContextMenu.Items.Add("Off", null, new EventHandler(OffMenuItem_Click));

            // Separation Line
            this.trayIconContextMenu.Items.Add(new ToolStripSeparator());

            // About Form Line
            aboutMenuItem = new ToolStripMenuItem();
            this.aboutMenuItem.Name = "aboutMenuItem";
            this.aboutMenuItem.Text = "About";
            this.aboutMenuItem.Click += new EventHandler(this.aboutMenuItem_Click);
            this.trayIconContextMenu.Items.Add(aboutMenuItem);

            // Separation Line
            this.trayIconContextMenu.Items.Add(new ToolStripSeparator());

            // 
            // CloseMenuItem
            // 
            closeMenuItem = new ToolStripMenuItem();
            this.closeMenuItem.Name = "CloseMenuItem";
            //this.closeMenuItem.Size = new Size(152, 22);
            this.closeMenuItem.Text = "Exit";
            this.closeMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);
            this.trayIconContextMenu.Items.Add(closeMenuItem);

            trayIconContextMenu.ResumeLayout(false);
            trayIcon.ContextMenuStrip = trayIconContextMenu;


        }

        void GetLyncClient()
        {
            try
            {
                lyncClient = LyncClient.GetClient();
                lyncClient.StateChanged += lyncClient_StateChanged;
                
                if (lyncClient.State == ClientState.SignedIn)
                    lyncClient.Self.Contact.ContactInformationChanged += Contact_ContactInformationChanged;
    
                SetCurrentContactState();
            }
            catch (ClientNotFoundException)
            {
                SetLyncIntegrationMode(false);
                trayIcon.ShowBalloonTip(1000, "Error", "Lync Client not started. Running in manual mode now. Please use the context menu to change your blink color", ToolTipIcon.Warning);
            }
            catch (Exception e)
            {
                trayIcon.ShowBalloonTip(1000, "Error", "Something went wrong by getting your Lync status. Running in manual mode now. Please use the context menu to change your blink color", ToolTipIcon.Warning);
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
            Rgb newColor = colorOff;
            if (lyncClient.State == ClientState.SignedIn)
            {
                ContactAvailability currentAvailability = (ContactAvailability)lyncClient.Self.Contact.GetContactInformation(ContactInformationType.Availability);
                switch (currentAvailability)
                {
                    case ContactAvailability.Busy:
                        newColor = colorBusy;
                        break;
                    case ContactAvailability.Free:
                        newColor = colorAvailable;
                        break;
                    case ContactAvailability.Away:
                        newColor = colorAway;
                        break;
                    case ContactAvailability.DoNotDisturb:
                        newColor = colorBusy;
                        break;
                    default:
                        break;
                }

                SetBlink1State(newColor);
            }
        }

        void SetBlink1State(Rgb color)
        {
            if ( blink1.IsConnected )
                blink1.SetColor(color);
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

            // Close blink Connection and switch off LED
            if (blink1.IsConnected)
                blink1.Close();
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

        private void OffMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorOff);
        }

        private void AwayMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorAway);
        }

        private void BusyMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorBusy);
        }

        private void AvailableMenuItem_Click(object sender, EventArgs e)
        {
            SetBlink1State(colorAvailable);
        }
    }
}
