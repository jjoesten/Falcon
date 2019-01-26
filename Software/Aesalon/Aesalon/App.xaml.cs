using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Aesalon
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            if (!CheckUniqueInstance())
            {
                Shutdown();
                return;
            }

            try
            {
                ConfigHolder.Singleton.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Translations.Main.ConfigLoadErrorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            FalconConnector.Singleton.Start();
        }

        private Mutex uniqueInstanceMutex;
        private bool CheckUniqueInstance()
        {
            uniqueInstanceMutex = new Mutex(false, "Aesalon");
            return uniqueInstanceMutex.WaitOne(TimeSpan.FromSeconds(0), false);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            using (StreamWriter writer = new StreamWriter("Crashlog.txt", append: true))
            {
                writer.WriteLine(string.Format("{0}: {1}", DateTime.Now, e.Exception));
                writer.WriteLine();
            }
        }
    }
}
