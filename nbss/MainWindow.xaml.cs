using System.Diagnostics;
using System.Windows;

namespace nbss
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                string apiKey = Properties.Settings.Default.APIKey;

                if (string.IsNullOrWhiteSpace(apiKey))
                {
                    var prompt = new APIKeyDialog();
                    if (prompt.ShowDialog() == true)
                    {
                        apiKey = prompt.APIKey;
                        Properties.Settings.Default.APIKey = apiKey;
                        Properties.Settings.Default.Save();
                    }
                    else
                    {
                        Application.Current.Shutdown();
                        return;
                    }
                }

                TorboxAPI.SetAPIKey(apiKey);
            };
        }
    }
}
