namespace K_C_S_J
{
    using Caliburn.Micro;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Threading;
    using ViewModels;

    public class AppBootstrapper : BootstrapperBase
    {
        private readonly SimpleContainer _container = new SimpleContainer();

        public AppBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container.Instance<SimpleContainer>(_container);

            _container.Singleton<IWindowManager, WindowManager>(key: nameof(WindowManager))
                .Singleton<IEventAggregator, EventAggregator>(key: nameof(EventAggregator));

            _container.Singleton<ShellViewModel>(key: nameof(ShellViewModel))
                .Singleton<MainViewModel>(key: nameof(MainViewModel))
                .Singleton<SettingViewModel>(key: nameof(SettingViewModel))
                .PerRequest<MessageViewModel>(key: nameof(MessageViewModel))
                .PerRequest<SmoothInputViewModel>(key: nameof(SmoothInputViewModel))
                .PerRequest<PeaksInputViewModel>(key: nameof(PeaksInputViewModel));
        }

        protected override void OnStartup(object s, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override void OnExit(object s, EventArgs e)
        {
            // 保存程序设置
            Properties.Settings.Default.Save();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }

        protected override void OnUnhandledException(object s, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK);
        }
    }
}
