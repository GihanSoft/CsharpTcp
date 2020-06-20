using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CsTcp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Socket socket;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnDataReceived()
        {
            Dispatcher.Invoke(() =>
            {
                btnSend.IsEnabled = false;
            });
            MemoryStream memoryStream = new MemoryStream();
            while (socket.Available > 0)
            {
                var buffer = new byte[socket.Available];
                socket.Receive(buffer.AsSpan());
                memoryStream.Write(buffer);
            }

            Dispatcher.Invoke(() =>
            {
                memoryStream.Position = 0;
                var msg = new StreamReader(memoryStream).ReadToEnd();

                var bubble = new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Background = new SolidColorBrush(Color.FromRgb(200, 255, 235)),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(5),
                    Margin = new Thickness(50, 5, 5, 5),
                    Text = "Server\r\n" + msg
                };
                sp.Children.Add(bubble);

                btnSend.IsEnabled = true;
            });
        }

        private void BtnConnnection_Click(object sender, RoutedEventArgs e)
        {
            if (socket?.Connected ?? false)
            {
                btnConnnection.Content = "Connect";
                btnConnnection.IsEnabled = false;

                txtAddress.IsReadOnly = false;
                txtPort.IsReadOnly = false;

                txtMsg.IsReadOnly = true;
                btnSend.IsEnabled = false;

                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                socket.Dispose();
                socket = null;

                var bubble = new TextBox
                {
                    TextWrapping = TextWrapping.Wrap,
                    IsReadOnly = true,
                    Background = new SolidColorBrush(Color.FromRgb(255, 235, 235)),
                    BorderThickness = new Thickness(0),
                    Padding = new Thickness(5),
                    Margin = new Thickness(50, 5, 5, 5),
                    Text = "=============================================================="
                };
                sp.Children.Add(bubble);
                btnConnnection.IsEnabled = true;
            }
            else
            {
                btnConnnection.IsEnabled = false;
                btnConnnection.Content = "Disconnect";

                txtAddress.IsReadOnly = true;
                txtPort.IsReadOnly = true;

                txtMsg.IsReadOnly = false;
                btnSend.IsEnabled = true;

                if(!IPAddress.TryParse(txtAddress.Text, out var host))
                {
                    host = Dns.GetHostAddresses(txtAddress.Text)[0];
                }
                var port = int.Parse(txtPort.Text);

                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(host, port);

                Task.Run(async () =>
                {
                    while (socket?.Connected ?? false)
                    {
                        if (socket.Available > 0)
                            OnDataReceived();
                        await Task.Delay(100);
                    }
                });
                btnConnnection.IsEnabled = true;
            }
        }

        private void txtMsg_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                btnSend_Click(this, null);
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (btnSend.IsEnabled == false)
                return;
            var msg = txtMsg.Text;
            txtMsg.Text = null;

            socket.Send(Encoding.UTF8.GetBytes(msg));

            var bubble = new TextBox
            {
                TextWrapping = TextWrapping.Wrap,
                IsReadOnly = true,
                Background = new SolidColorBrush(Color.FromRgb(200, 235, 255)),
                BorderThickness = new Thickness(0),
                Padding = new Thickness(5),
                Margin = new Thickness(5, 5, 50, 5),
                Text = "Client:\r\n" + msg
            };
            sp.Children.Add(bubble);
        }
    }
}
