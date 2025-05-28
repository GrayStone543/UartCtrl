using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Diagnostics;


namespace PumpUtil
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static bool sContinue = false;
        static SerialPort? sSerialPort;
        Thread? mReadThread;

        public MainWindow()
        {
            InitializeComponent();

            textBoxOutput.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            //textBoxOutput.Text = "Pump Utility Output:\nLine 2\nLine 3\n";
            //for (int i = 0; i< 10; i++)
            //{
            //    textBoxOutput.Text += $"Line {i + 4}\n";
            //}

            foreach (string s in SerialPort.GetPortNames())
            {
                comboBoxPorts.Items.Add(s);
            }

            foreach (string s in Enum.GetNames(typeof(Parity)))
            {
                comboBoxParity.Items.Add(s);
            }

            // Create a new SerialPort object with default settings.
            sSerialPort = new SerialPort();
            
            comboBoxParity.SelectedIndex = 0; // Default to None parity
            textBoxBaudRate.Text = "115200"; // Default baud rate
            textBoxDataBits.Text = "8"; // Default data bits
            textBoxStopBits.Text = "1"; // Default stop bits
        }

        public static void Read()
        {
            while (sContinue)
            {
                if (sSerialPort != null && sSerialPort.IsOpen)
                {
                    try
                    {
                        string data = sSerialPort.ReadLine();
                        // Process the data as needed
                        // For example, you can display it in a TextBox or log it
                        //Console.WriteLine(data); // Replace with your processing logic
                        Debug.WriteLine(data); // Use Debug for logging in development
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            // Update the UI with the received data
                            // Assuming you have a TextBox named textBoxOutput in your XAML
                            if (Application.Current.MainWindow is MainWindow mainWindow)
                            {
                                mainWindow.textBoxOutput.AppendText(data + "\n");
                                mainWindow.textBoxOutput.ScrollToEnd(); // Scroll to the end of the TextBox
                            }
                        });
                    }
                    catch (TimeoutException)
                    {
                        // Handle timeout exceptions if necessary
                    }
                    catch (Exception ex)
                    {
                        //Console.WriteLine($"Error reading from port: {ex.Message}");
                        Debug.WriteLine($"Error reading from port: {ex.Message}");
                    }
                }
            }
            Debug.WriteLine("### Read thread stopped.");
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (sSerialPort != null && !sSerialPort.IsOpen)
            {
                try
                {
                    sSerialPort.PortName = comboBoxPorts.SelectedItem.ToString();
                    sSerialPort.BaudRate = int.Parse(textBoxBaudRate.Text);
                    sSerialPort.DataBits = int.Parse(textBoxDataBits.Text);
                    sSerialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), textBoxStopBits.Text);
                    string parity = comboBoxParity.SelectedItem.ToString() ?? "None";
                    sSerialPort.Parity = (Parity)Enum.Parse(typeof(Parity), parity);
                    
                    sSerialPort.ReadTimeout = 500; // Set a read timeout of 500 ms
                    sSerialPort.WriteTimeout = 500; // Set a write timeout of 500 ms
                    
                    sSerialPort.Open();
                    buttonOpen.IsEnabled = false; // Disable the Open button after opening the port
                    sContinue = true; // Set the continue flag to true
                    mReadThread = new Thread(Read);
                    mReadThread.IsBackground = true; // Set the thread as a background thread
                    mReadThread.Start(); // Start the read thread
                    MessageBox.Show("Port opened successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening port: {ex.Message}");
                }
            }
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (sSerialPort != null && sSerialPort.IsOpen)
            {
                try
                {
                    sSerialPort.Close();
                    buttonOpen.IsEnabled = true; // Enable the Open button after closing the port
                    sContinue = false; // Set the continue flag to false to stop the read thread
                    MessageBox.Show("Port closed successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error closing port: {ex.Message}");
                }
            }
        }


        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (sSerialPort != null && sSerialPort.IsOpen)
            {
                try
                {
                    // Assuming you have a TextBox named textBoxSend for sending data
                    //string dataToSend = textBoxSend.Text
                    string dataToSend = "?\n";
                    sSerialPort.WriteLine(dataToSend);
                    Debug.WriteLine($"Sent: {dataToSend}");
                    //textBoxOutput.AppendText($"Sent: {dataToSend}\n");
                    //textBoxOutput.ScrollToEnd(); // Scroll to the end of the TextBox
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending data: {ex.Message}");
                }
            }
        }   
    }
}