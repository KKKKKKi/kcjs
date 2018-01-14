namespace K_C_S_J.ViewModels
{
    using Caliburn.Micro;
    using Models;

    public class PeaksInputViewModel : Screen
    {
        private readonly IEventAggregator _eventAggregator = IoC.Get<IEventAggregator>(nameof(EventAggregator));

        private string _startIndex = "", _endIndex = "", _numM = "", _numH = "", _numR = "";

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

        public string Numm
        {
            get => _numM;
            set => Set(ref _numM, value);
        }

        public string NumH
        {
            get => _numH;
            set => Set(ref _numH, value);
        }

        public string NumR
        {
            get => _numR;
            set => Set(ref _numR, value);
        }

        public void SendOpts()
        {
            if (!int.TryParse(_startIndex, out int start))
            {
                start = 0;
            }
            if (!int.TryParse(_endIndex, out int end))
            {
                end = 2048;
            }
            if (!int.TryParse(_numM, out int m))
            {
                m = 4;
            }
            if (!int.TryParse(_numH, out int h))
            {
                h = 4;
            }
            if (!double.TryParse(_numR, out double r))
            {
                r = 2;
            }

            PeakOpts p = new PeakOpts(start, end, m, h, r);
            _eventAggregator.PublishOnUIThread(p);
        }
    }
}
