using F4SharedMemoryMirror;

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

        private void ExecuteStartMirroring(object o)
        {
            Mirror.Singleton.StartMirroring();
        }
        private bool CanExecuteStartMirroring(object o)
        {
            return !Mirror.Singleton.IsRunning;
        }

        private void ExecuteStopMirroring(object o)
        {
            Mirror.Singleton.StopMirroring();
        }
        private bool CanExecuteStopMirroring(object o)
        {
            return Mirror.Singleton.IsRunning;
        }


        #endregion
    }
}
