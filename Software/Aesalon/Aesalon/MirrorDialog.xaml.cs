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

namespace Aesalon
{
    /// <summary>
    /// Interaction logic for MirrorDialog.xaml
    /// </summary>
    public partial class MirrorDialog : Window
    {
        public MirrorDialog()
        {
            InitializeComponent();
            DataContext = new MirrorViewModel();
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ConfigHolder.Singleton.Save();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, Translations.Main.ConfigSaveErrorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}
