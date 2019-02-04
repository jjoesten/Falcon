namespace Aesalon
{
    public class MirrorViewModel : BindableObject
    {
        public string Title { get { return Translations.Main.MirrorConfigure; } }

        public ConfigHolder ConfigHolder { get { return ConfigHolder.Singleton; } }

        public RelayCommand StartMirroringCommand { get; private set; }
        public RelayCommand StopMirroringCommand { get; private set; }

        public MirrorViewModel()
        {
            StartMirroringCommand = new RelayCommand(ExecuteStartMirroring, CanExecuteStartMirroring);
            StopMirroringCommand = new RelayCommand(ExecuteStopMirroring, CanExecuteStopMirroring);
        }

        #region Mirroring Commands

        #endregion
    }
}
