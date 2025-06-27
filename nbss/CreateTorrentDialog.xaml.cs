using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace nbss
{
    /// <summary>
    /// Interaction logic for CreateTorrentDialog.xaml
    /// </summary>
    public partial class CreateTorrentDialog : Window
    {
        public CreateTorrentResponse response;
        public CreateTorrentDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private async void OnOk_Button_Click(object sender, RoutedEventArgs e)
        {
            string urlText = urlTextBox.Text;
            string magnetText = magnetTextBox.Text;
            string seedText = seedTextBox.Text;
            var seed = 0;
            bool allowZip = allowZipCheck.IsChecked ?? false;
            bool asQueued = asQueuedCheck.IsChecked ?? false;
            string name = nameTextBox.Text;

            messageTextBlock.Text = string.Empty;
            if (string.IsNullOrEmpty(urlText) && string.IsNullOrEmpty(magnetText))
            {
                messageTextBlock.Text = "You must specify either a URL or magnet!";
            }

            if (!string.IsNullOrEmpty(seedText))
            {
                if (!int.TryParse(seedText, out seed))
                {
                    seed = 0;
                }
            }
            else
            {
                seed = 0;
            }

            outputText.Text = "Creating torrent...";
            response = await TorboxAPI.CreateTorrentAsync(urlText, magnetText, name, allowZip, asQueued, seed);

            DialogResult = true;
            Close();
        }
    }
}
