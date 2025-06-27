using System.IO;
using System.Windows;

namespace nbss
{
    /// <summary>
    /// Interaction logic for APIKeyDialog.xaml
    /// </summary>
    public partial class APIKeyDialog : Window
    {
        public string APIKey { get; private set; }
        public APIKeyDialog()
        {
            InitializeComponent();
            Owner = Application.Current.MainWindow;
        }

        private void OnOk_Button_Click(object sender, RoutedEventArgs e)
        {
            var path = keyTextBlock.Text;
            messageTextBlock.Text = string.Empty;
            if (string.IsNullOrEmpty(path))
            {
                messageTextBlock.Text = "Invalid API Key!";
            } 
            else
            {
                APIKey = path;
                DialogResult = true;
                Close();
            }
        }
    }
}
