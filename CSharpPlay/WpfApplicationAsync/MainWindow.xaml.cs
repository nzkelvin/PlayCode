using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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

namespace WpfApplicationAsync
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _count = 0;

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
        }

        private void btnIncreaseCount_Click(object sender, RoutedEventArgs e)
        {
            _count++;
            lblCountValue.Content = _count;
        }

        private void btnDownloadRss_Click(object sender, RoutedEventArgs e)
        {
            var client = new WebClient();
            //var data = client.DownloadString("http://www.filipekberg.se/rss/");
            client.DownloadStringAsync(new Uri("http://www.filipekberg.se/rss/"));
            client.DownloadStringCompleted += Client_DownloadStringCompeleted;
            //Thread.Sleep(10000);
        }

        private void Client_DownloadStringCompeleted(object sender, DownloadStringCompletedEventArgs e)
        {
            txtRss.Text = e.Result;
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {


            try
            {
                var result = await AsyncLogin();
                btnLogin.Content = result;
            }
            catch(Exception)
            {
                btnLogin.Content = "Login Exception";
            }
            
        }

        private async Task<string> AsyncLogin(){
            //try
            //{
                var result = await Task.Run(() =>
                {
                    Thread.Sleep(2000);
                    throw new UnauthorizedAccessException();
                    return "Login successful";
                    
                });

                return result; // return the state machine immidiately 
            //}
            //catch(Exception) {
            //    return "Login Failed";
            //}
           
        }
    }
}
