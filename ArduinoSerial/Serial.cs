using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Ports;
using System.Diagnostics;

namespace ArduinoSerialLeds
{
    public class Serial : IDisposable
    {
        SerialPort serialPort;

        public Serial()
        {
            serialPort = new SerialPort();
        }

        public void Init(string port)
        {
            try
            {
                serialPort.PortName = port;
                serialPort.BaudRate = 9600;
                serialPort.ReadTimeout = 1000;
                serialPort.WriteTimeout = 1000;
                serialPort.NewLine = "\n";
                serialPort.Open();
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        public SerialPort Port
        {
            get { return serialPort; }
        }

        public void Dispose()
        {
            serialPort.Close();
            serialPort.Dispose();
        }

        public void SetLEDs(byte[] colors)
        {
            try
            {
                //if (!Port.IsOpen) throw new Exception("Serial port not open.");

                //string response = string.Empty;

                serialPort.WriteLine(string.Join(",", colors));

                //response = serialPort.ReadLine();

                //Debug.WriteLine("{0} --> {1}", string.Join(",", rgb), response);
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
