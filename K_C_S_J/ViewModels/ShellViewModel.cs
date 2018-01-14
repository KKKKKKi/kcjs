namespace K_C_S_J.ViewModels
{
    using Caliburn.Micro;
    using System.Windows.Controls;

    public class ShellViewModel : Screen
    {
        private readonly SimpleContainer _container = IoC.Get<SimpleContainer>();
        private INavigationService _navigationService;

        public void WindowLoaded()
        {
            // 语言设置
        }
        
        public void RegisterFrame(Frame frame)
        {
            _navigationService = new FrameAdapter(frame);

            // _container.Instance(_navigationService);
            // INavigationService _navigationService = IoC.Get<INavigationService>(key: "ContentFrame");
            _container.RegisterInstance(typeof(INavigationService), "ContentFrame", _navigationService);
            _navigationService.NavigateToViewModel(typeof(MainViewModel), null);
        }
    }
}
