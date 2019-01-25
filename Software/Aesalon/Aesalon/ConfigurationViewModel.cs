using System;
using System.Reflection;
using System.Windows;

namespace Aesalon
{
    public class ConfigurationViewModel : BindableObject
    {
        private readonly Window view;

        #region Public Properties

        public string Title
        {
            get
            {
                Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version;
                var title = string.Format("{0} {1}.{2}.{3}", Translations.Main.ApplicationTitle, assemblyVersion.Major, assemblyVersion.Minor, assemblyVersion.Build);
#if DEBUG
                title += " DEBUG";
#endif
                return title;
            }
        }

        public ConfigHolder ConfigHolder { get { return ConfigHolder.Singleton; } }

        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand LoadCommand { get; private set; }

        // TODO: FalconLight list
        // TODO: FalconGauge list

        #endregion

        public ConfigurationViewModel(Window view)
        {
            this.view = view;
            SaveCommand = new RelayCommand(ExecuteSave);
            LoadCommand = new RelayCommand(ExecuteLoad);
        }

        private void ExecuteLoad(object o)
        {
            try
            {
                ConfigHolder.Load();
            }
            catch (Exception e)
            {
                MessageBox.Show(view, e.Message, Translations.Main.ConfigLoadErrorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSave(object o)
        {
            try
            {
                ConfigHolder.Save();
            }
            catch (Exception e)
            {
                MessageBox.Show(view, e.Message, Translations.Main.ConfigSaveErrorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                throw;
            }
        }
    }
}