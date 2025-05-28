using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;


namespace CLIUtil
{
    internal class PortChat
    {
        static bool sContinue = false;
        static SerialPort? sSerialPort;

        public void Start()
        {
            Thread readThread = new Thread(Read);

            string portName = SelectComPort();
            // Create a new SerialPort object with the specified parameters
            sSerialPort = new SerialPort(portName, 115200, Parity.None, 8, StopBits.One)
            {
                //ReadTimeout = 500,
                //WriteTimeout = 500,
                Handshake = Handshake.XOnXOff
            };

            try
            {
                sSerialPort.Open();
                Console.WriteLine($"Connected to {sSerialPort.PortName}");
                sContinue = true;
                readThread.Start();
                Console.WriteLine("Type messages to send. Type 'exit' to quit.");
                while (sContinue)
                {
                    string? input = Console.ReadLine();
                    if (input?.ToLower() == "exit")
                    {
                        sContinue = false;
                        break;
                    }
                    if (!string.IsNullOrEmpty(input))
                    {
                        //sSerialPort.WriteLine(input);
                        sSerialPort.Write(input + "\r"); // "\r" is the terminating symbol of STM32 device
                        Console.WriteLine("Sent: " + input);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            finally
            {
                sSerialPort.Close();
                readThread.Join();
                Console.WriteLine("Connection closed.");
            }
        }

        private string SelectComPort()
        {
            Console.WriteLine("Available Serial Ports:");
            string[] ports = SerialPort.GetPortNames();
            for (int i = 0; i < ports.Length; i++)
            {
                Console.WriteLine($"{i}) {ports[i]}");
            }
            Console.Write("Select a port by number: ");
            string? input = Console.ReadLine();
            int sel = int.Parse(input ?? "0");
            while (sel < 0 || sel >= ports.Length)
            {
                Console.Write("Invalid selection. Select again:");
                input = Console.ReadLine();
                sel = int.Parse(input ?? "0");
            }

            return ports[sel];
        }

        public static void Read()
        {
            Console.WriteLine("### Read thread start");
            while (sContinue)
            {
                try
                {
                    if (sSerialPort != null && sSerialPort.IsOpen) {
                        //Console.WriteLine("Waiting for data...");
                        string? message = sSerialPort.ReadLine();
                        Console.WriteLine("Received: " + message);
                    }
                }
                catch (TimeoutException)
                {
                    Console.WriteLine("### Read timeout occurred. No data received.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error reading from port: " + ex.Message);
                }
            }
            Console.WriteLine("### Read thread end");
        }
    }
}
