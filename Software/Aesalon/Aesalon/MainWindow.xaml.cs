using Hardcodet.Wpf.TaskbarNotification;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Aesalon
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ConfigurationDialog configurationDialog;
        private MirrorDialog mirrorDialog;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            //TaskbarIcon.ShowBalloonTip(Translations.Main.ApplicationTitle, Translations.Main.ApplicationStartedBalloonText, BalloonIcon.Info)
        }

        private void MenuItemConfigure_Click(object sender, RoutedEventArgs e)
        {
            if (configurationDialog == null)
            {
                MenuItemQuit.IsEnabled = false;
                configurationDialog = new ConfigurationDialog();
                configurationDialog.ShowDialog();
                configurationDialog = null;
                MenuItemQuit.IsEnabled = true;
            }
            else
            {
                configurationDialog.Activate();
            }
        }

        private void MenuItemQuit_Click(object sender, RoutedEventArgs e)
        {
            if (configurationDialog == null)
                Close();
        }

        private void MenuItemMirror_Click(object sender, RoutedEventArgs e)
        {
            if (mirrorDialog == null)
            {
                mirrorDialog = new MirrorDialog();
                mirrorDialog.ShowDialog();
                mirrorDialog = null;
            }
            else
            {
                mirrorDialog.Activate();
            }
        }
    }
}
