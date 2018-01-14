namespace K_C_S_J.ViewModels
{
    using Caliburn.Micro;
    using System.Collections.Generic;
    using MaterialDesignThemes.Wpf;
    using Models;

    public class SmoothInputViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator = IoC.Get<IEventAggregator>(nameof(EventAggregator));

        private string _startIndex = "", _endIndex = "";

        public string StartIndex
        {
            get => _startIndex;
            set => Set(ref _startIndex, value);
        }

        public string EndIndex
        {
            get => _endIndex;
            set => Set(ref _endIndex, value);
        }

        private int _selectedOpt = 3;

        public int SelectedOpt
        {
            get => _selectedOpt;
            set => Set(ref _selectedOpt, value);
        }

        public List<int> Opts => new List<int> { 3, 5, 7 };

        public void SendOpts()
        {
            if (!int.TryParse(_startIndex, out int start))
            {
                /* 不能打开多个对话框
                await DialogHost.Show(new Views.MessageView()
                {
                    Message = { Text = "error input!" }
                }, "RootDialog");
                return;
                */
                start = 0;
            }
            if (!int.TryParse(_endIndex, out int end))
            {
                /*
                await DialogHost.Show(new Views.MessageView()
                {
                    Message = { Text = "error input!" }
                }, "RootDialog");
                return;
                */
                end = 2048;
            }
            SmoothOpts opts = new SmoothOpts(start, end, _selectedOpt);
            _eventAggregator.PublishOnUIThread(opts);
        }
    }
}
