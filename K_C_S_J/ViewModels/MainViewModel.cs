namespace K_C_S_J.ViewModels
{
    using Caliburn.Micro;
    using MaterialDesignThemes.Wpf;
    using OxyPlot;
    using OxyPlot.Axes;
    using OxyPlot.Series;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Models;
    using Microsoft.Win32;

    public class MainViewModel : Screen, IHandle<SmoothOpts>, IHandle<PeakOpts>
    {
        private readonly SimpleContainer _container = IoC.Get<SimpleContainer>();
        private readonly IWindowManager _windowManager = IoC.Get<IWindowManager>(nameof(WindowManager));
        private readonly IEventAggregator _eventAggregator = IoC.Get<IEventAggregator>(nameof(EventAggregator));

        private ISnackbarMessageQueue _snackbarMessageQueue;

        public MainViewModel()
        {
            _eventAggregator.Subscribe(this);
            InitController();
            InitPlotModel();
        }

        public void RegisterMessageQueue(Snackbar s)
        {
            _snackbarMessageQueue = s.MessageQueue;
            _container.RegisterInstance(typeof(ISnackbarMessageQueue), "MainSnackbarMessageQueue", _snackbarMessageQueue);
        }

        #region Source Data

        /// <summary>
        /// 读取的谱数据
        /// </summary>
        private List<DataPoint> _mca, _pu;

        /// <summary>
        /// 当前的谱数据
        /// </summary>
        private List<DataPoint> _points;

        #endregion

        #region read files

        public void ReadPu12()
        {
            RemoveAllSeriesFromPlotModel();
            _pAreaSeries.Clear();

            try
            {
                using(FileStream fs = new FileStream("./Files/12.pu", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        _pu = new List<DataPoint>();
                        string oneLine = "";
                        double count;
                        for (int i = 0; i < 2047; i++)
                        {
                            oneLine = reader.ReadLine();
                            count = double.Parse(oneLine);
                            _pu.Add(new DataPoint(i, count));
                        }
                        AddDataToSeries(_pu);
                    }
                }
                SetPlotModelTitle("12.pu");
                SetSeriesTitle("12");
            }
            catch (Exception error)
            {
                ShowMessage(error.Message);
                // MessageBox.Show(error.Message, "Error!好像出现了什么不明错误", MessageBoxButton.OK);
                // throw;
            }
        }
        public void ReadPu28()
        {
            RemoveAllSeriesFromPlotModel();
            _pAreaSeries.Clear();

            try
            {
                using (FileStream fs = new FileStream("./Files/28.pu", FileMode.Open, FileAccess.Read))
                {
                    using (StreamReader reader = new StreamReader(fs))
                    {
                        _pu = new List<DataPoint>();
                        string oneLine = "";
                        double count;
                        for (int i = 0; i < 2047; i++)
                        {
                            oneLine = reader.ReadLine();
                            count = double.Parse(oneLine);
                            _pu.Add(new DataPoint(i, count));
                        }
                        AddDataToSeries(_pu);
                    }
                }
                SetPlotModelTitle("28.pu");
                SetSeriesTitle("28");
            }
            catch (Exception error)
            {
                ShowMessage(error.Message);
                // MessageBox.Show(error.Message, "Error!好像出现了什么不明错误", MessageBoxButton.OK);
                // throw;
            }
        }

        public void ReadMCA()
        {
            RemoveAllSeriesFromPlotModel();
            _pAreaSeries.Clear();

            try
            {
                using (FileStream fs = new FileStream("./Files/Gss5-6.mca", FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader reader = new BinaryReader(fs))
                    {
                        _mca = new List<DataPoint>();
                        string header = reader.ReadString(230);
                        for (int i = 0; i < 1024; i++)
                        {
                            int count = reader.ReadInt32();
                            _mca.Add(new DataPoint(i, count));
                        }
                        AddDataToSeries(_mca);
                    }
                }
                SetPlotModelTitle("Gss5-6.mca");
                SetSeriesTitle("Gss5-6");
            }
            catch (Exception error)
            {
                ShowMessage(error.Message);
                // MessageBox.Show(error.Message, "Error!好像出现了什么不明错误", MessageBoxButton.OK);
                // throw;
            }
        }

        public void SaveAs()
        {
            try
            {
                SaveFileDialog dlg = new SaveFileDialog()
                {
                    Title = "保存谱数据",
                    Filter = "CSV|*.csv",
                    FilterIndex = 0,
                    FileName = DateTime.Now.ToString("yyyyMMddhhmmss"),
                };
                dlg.FileOk += async (s, e) =>
                {
                    if (_points == null || _points.Count == 0)
                    {
                        return;
                    }
                    string content = "";
                    for (int i = 0; i < _points.Count; i++)
                    {
                        content += $"{i + 1}, {_points[i].Y}" + Environment.NewLine;
                    }

                    using (StreamWriter sw = new StreamWriter(dlg.FileName))
                    {
                        await sw.WriteAsync(content);
                    }

                    ShowMessage("文件保存成功");
                };

                dlg.ShowDialog();
            }
            catch (Exception e)
            {
                ShowErrorDialog(e.Message);
                // throw;
            }
        }

        #endregion

        #region PlotModel

        private readonly PlotModel _plotModel = new PlotModel() { Title = "Welcome" };

        public PlotModel DataModel => _plotModel;

        /// <summary>
        /// <see cref="Controller"/>
        /// </summary>
        private readonly PlotController _controller = new PlotController();

        /// <summary>
        /// PlotController of PlotModel
        /// </summary>
        public PlotController Controller => _controller;

        private LinearAxis _channelAxis, _countAxis;

        private readonly LineSeries _lineSeries = new LineSeries()
        {
            Color = OxyColors.Transparent,
            MarkerType = MarkerType.Circle,
            MarkerFill = OxyColors.DodgerBlue,
            MarkerSize = 2
        };

        private readonly List<AreaSeries> _pAreaSeries = new List<AreaSeries>();

        private void InitPlotModel()
        {
            _channelAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                Title = "Channel",
                AbsoluteMinimum = -1,
            };
            _countAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                Title = "Count",
                // AbsoluteMinimum = 0,
                // IsZoomEnabled = false,
                // IsPanEnabled = false,
            };
            _plotModel.Axes.Add(_channelAxis);
            _plotModel.Axes.Add(_countAxis);
            AddSeriesToPlotModel(_lineSeries);
        }

        private void InitController()
        {
            KeyUp = new DelegatePlotCommand<OxyKeyEventArgs>(
                (view, controller, e) =>
                {
                    e.Handled = true;
                    view.ActualModel.DefaultYAxis.IsZoomEnabled = true;
                    view.ActualModel.DefaultYAxis.ZoomAt(1.1, 0);
                    view.ActualModel.DefaultYAxis.IsZoomEnabled = false;
                    view.InvalidatePlot(false);
                });
            KeyDown = new DelegatePlotCommand<OxyKeyEventArgs>(
                (view, controller, e) =>
                {
                    e.Handled = true;
                    view.ActualModel.DefaultYAxis.IsZoomEnabled = true;
                    view.ActualModel.DefaultYAxis.ZoomAt(.9, 0);
                    view.ActualModel.DefaultYAxis.IsZoomEnabled = false;
                    view.InvalidatePlot(false);
                });
            _controller.BindKeyDown(OxyKey.Up, KeyUp);
            _controller.BindKeyDown(OxyKey.Down, KeyDown);
            // _controller.BindKeyDown(OxyKey.R, PlotCommands.Reset);
        }

        public IViewCommand<OxyKeyEventArgs> KeyUp, KeyDown;
        

        /// <summary>
        /// 设置PlotModel标题
        /// </summary>
        /// <param name="title">标题</param>
        public void SetPlotModelTitle(string title)
        {
            _plotModel.Title = title;
        }

        /// <summary>
        /// 设置Series标题
        /// </summary>
        /// <param name="title">标题</param>
        public void SetSeriesTitle(string title)
        {
            _lineSeries.Title = title;
        }

        public void SetAsixRange(int min, int max)
        {
            _channelAxis.AbsoluteMinimum = min;
            _channelAxis.AbsoluteMaximum = max;
        }

        public void SetAsiyRange(int min, int max)
        {
            _countAxis.AbsoluteMinimum = min;
            _countAxis.AbsoluteMaximum = max;
        }
        public void SetAsiyRange(double min, double max)
        {
            _countAxis.AbsoluteMinimum = min;
            _countAxis.AbsoluteMaximum = max;
        }

        private void AddDataToSeries(List<DataPoint> points)
        {
            _points = points;
            _lineSeries.Points.Clear();
            _lineSeries.Points.AddRange(_points);
            SetAsixRange(-1, _points.Count);
            _plotModel.InvalidatePlot(true);
        }

        /// <summary>
        /// AddSeriesToPlotModel
        /// </summary>
        /// <param name="series">Series</param>
        private void AddSeriesToPlotModel(Series series)
        {
            _plotModel.Series.Add(series);
            _plotModel.InvalidatePlot(true);
        }

        private void RemoveSeriesFromPlotModel(int index)
        {
            _plotModel.Series.RemoveAt(index);
        }

        private void RemoveSeriesFromPlotModel(Series series)
        {
            if (_plotModel.Series.Contains(series))
            {
                _plotModel.Series.Remove(series);
            }
        }

        /// <summary>
        /// 添加所有的峰绘图
        /// </summary>
        private void AddPeaksToPlotModel()
        {
            foreach (Peak p in _peaks)
            {
                List<DataPoint> points1 = new List<DataPoint>();
                List<DataPoint> points2 = new List<DataPoint>();
                AreaSeries series = new AreaSeries() { ToolTip = $"Area: {p.Area}", Fill = OxyColors.LimeGreen, LineStyle = LineStyle.None };

                for (int i = p.EdgeLeft; i <= p.EdgeRight; i++)
                {
                    points1.Add(_points[i]);
                    points2.Add(new DataPoint(i, 0));
                }

                series.Points.AddRange(points1);
                series.Points2.AddRange(points2);

                _pAreaSeries.Add(series);
                AddSeriesToPlotModel(series);
            }
        }

        /// <summary>
        /// 从画面中移除所有的峰
        /// </summary>
        private void RemoveAllSeriesFromPlotModel()
        {
            _plotModel.Series.Clear();
            _plotModel.Series.Add(_lineSeries);
            _plotModel.InvalidatePlot(true);
        }

        public void SaveModelAs(int width = 1280, int height = 720)
        {
            try
            {
                string fn = ".\\Saves\\" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
                OxyPlot.Wpf.PngExporter.Export(_plotModel, fn, width, height, OxyColors.White);
                ShowMessage("保存图像成功");
            }
            catch (Exception e)
            {
                ShowErrorDialog(e.Message);
                // throw;
            }
        }

        #endregion

        #region 重心法

        public void Handle(SmoothOpts message)
        {
            StartSmooth(message.StartIndex, message.EndIndex, message.Opt);
        }

        private void StartSmooth(int startIndex, int endIndex, int opt)
        {
            if (_points == null || _points.Count == 0)
            {
                return;
            }
            if (opt == 3)
            {
                Smooth3(startIndex, endIndex);
            }
            else if (opt == 5)
            {
                Smooth5(startIndex, endIndex);
            }
            else if (opt == 7)
            {
                Smooth7(startIndex, endIndex);
            }
            _lineSeries.Points.Clear();
            _lineSeries.Points.AddRange(_points);
            _plotModel.InvalidatePlot(true);
        }

        private void Smooth3(int startIndex = 0, int endIndex = 2048)
        {
            if (_points == null || _points.Count == 0)
            {
                return;
            }

            int pCount = _points.Count;

            if (startIndex >= pCount || startIndex >= endIndex)
            {
                ShowMessage("输入参数错误！");
                return;
            }

            if (startIndex < 1)
            {
                startIndex = 1;
            }
            if (endIndex > pCount - 2)
            {
                endIndex = pCount - 2;
            }

            DataPoint[] dataArray = new DataPoint[pCount];
            double tempNum = 0;

            for (int i = 0; i < pCount; i++)
            {
                if (i < startIndex || i > endIndex)
                {
                    dataArray[i] = _points[i];
                    continue;
                }
                tempNum = (_points[i - 1].Y + _points[i + 1].Y + 2 * _points[i].Y) / 4;
                dataArray[i] = new DataPoint(i, tempNum);
            }

            _points = new List<DataPoint>(dataArray);
            // other
        }

        private void Smooth5(int startIndex = 0, int endIndex = 2048)
        {
            if (_points == null || _points.Count == 0)
            {
                return;
            }

            int pCount = _points.Count;

            if (startIndex >= pCount || startIndex >= endIndex)
            {
                ShowMessage("输入参数错误！");
                return;
            }

            if (startIndex < 2)
            {
                startIndex = 2;
            }
            if (endIndex > pCount - 3)
            {
                endIndex = pCount - 3;
            }

            DataPoint[] dataArray = new DataPoint[pCount];
            double tempNum = 0;

            for (int i = 0; i < pCount; i++)
            {
                if (i < startIndex || i > endIndex)
                {
                    dataArray[i] = _points[i];
                    continue;
                }

                tempNum = (_points[i - 2].Y + _points[i + 2].Y + 4 * _points[i - 1].Y + 4 * _points[i + 1].Y + 6 * _points[i].Y) / 16;
                dataArray[i] = new DataPoint(i, tempNum);
            }

            _points = new List<DataPoint>(dataArray);
            // other
        }

        private void Smooth7(int startIndex = 0, int endIndex = 2048)
        {
            if (_points == null || _points.Count == 0)
            {
                return;
            }

            int pCount = _points.Count;

            if (startIndex >= pCount || startIndex >= endIndex)
            {
                ShowMessage("输入参数错误！");
                return;
            }

            if (startIndex < 3)
            {
                startIndex = 3;
            }
            if (endIndex > pCount - 4)
            {
                endIndex = pCount - 4;
            }

            DataPoint[] dataArray = new DataPoint[pCount];
            double tempNum = 0;

            for (int i = 0; i < pCount; i++)
            {
                if (i < startIndex || i > endIndex)
                {
                    dataArray[i] = _points[i];
                    continue;
                }

                tempNum = (_points[i - 3].Y + _points[i + 3].Y + 6 * _points[i - 2].Y + 6 * _points[i + 2].Y + 15 * _points[i - 1].Y + 15 * _points[i + 1].Y + 20 * _points[i].Y) / 64;
                dataArray[i] = new DataPoint(i, tempNum);
            }

            _points = new List<DataPoint>(dataArray);
            // other
        }

        #endregion

        #region 协方差法（柯西函数）

        public void Handle(PeakOpts message)
        {
            // RemoveAllSeriesFromPlotModel();
            Peaks(message.StartIndex, message.EndIndex, message.NumH, message.Numm, message.NumR);
            // _pAreaSeries.Clear();
            // AddPeaksToPlotModel();
            //
        }

        /// <summary>
        /// 找到的峰
        /// </summary>
        private List<Peak> _peaks = new List<Peak>();

        private double Gj(int i, int j, int m)
        {
            return 1.0;
        }

        private double Cj(int j, int h)
        {
            return h * h / (double)(h * h + 4 * j * j);
        }

        private double Gauss(int j, int h)
        {
            return Math.Exp(-4 * Math.Log(2) * j * j / (double)(h * h));
        }

        private double Ri(int i,int h, int m)
        {
            double y1, y2;

            double g = 0, c = 0, d = 0, G = 0, gcd = 0, gxC = 0, gd = 0, gxC2 = 0;

            for (int j = -m; j <= m; j++)
            {
                d = _points[i + j].Y;
                g = Gj(i, j, m);
                c = Gauss(j, h);

                G += g;
                gxC += g * c;
                gxC2 += g * c * c;
                gcd += g * c * d;
                gd += g * d;
            }

            y1 = G * gcd - gxC * gd;
            y2 = G * (G * gxC2 - gxC * gxC);
            y2 = Math.Sqrt(y2);

            return y1 / y2;
        }

        /// <summary>
        /// 寻峰函数
        /// </summary>
        /// <param name="startIndex">起始道址</param>
        /// <param name="endIndex">结束道址</param>
        /// <param name="h">半高宽</param>
        /// <param name="m">点数</param>
        /// <param name="R">阈值</param>
        private void Peaks(int startIndex, int endIndex, int h, int m, double R)
        {
            // 当前没有谱线，直接返回
            if (_points == null || _points.Count == 0)
            {
                return;
            }

            // 当前谱线的总道数
            int pCount = _points.Count;

            // 输入参数是否符合要求
            // 不符合时提示错误，并返回
            if (startIndex >= pCount || startIndex >= endIndex)
            {
                ShowMessage("输入参数错误！");
                return;
            }

            // 除去谱线边界的点
            if (startIndex < m)
            {
                startIndex = m;
            }
            if (endIndex > pCount - m - 2)
            {
                endIndex = pCount - m - 2;
            }

            double[] ris = new double[endIndex - startIndex + 1];
            double ri;

            // 计算对应道址的Ri
            for (int i = startIndex; i <= endIndex; i++)
            {
                ri = Ri(i, h, m);
                ris[i - startIndex] = ri;
            }

            //* 用于显示对应的Ri曲线，仅供调试使用，编译时应当注释掉
            List<DataPoint> lRi = new List<DataPoint>();
            for (int i = 0; i < ris.Length; i++)
            {
                lRi.Add(new DataPoint(startIndex + i, ris[i]));
            }
            LineSeries seriesRi = new LineSeries() { Title = $"H={h} m={m}" };
            seriesRi.Points.AddRange(lRi);

            _plotModel.Series.Add(seriesRi);
            _plotModel.InvalidatePlot(true);
            //*/

            //* 峰的判别
            for (int i = 1; i < ris.Length - 1; i++)
            {
                // 小于R时 直接跳到下一个循环
                if (ris[i] <= R) continue;

                // 认为找到了峰
                Peak p = new Peak();

                int min = i, max = i;

                for (int j = i + 1; j < ris.Length; j++)
                {
                    if (ris[j] < ris[max]) break;
                    max = j;
                }
                p.Position = max + startIndex;

                for (int j = min - 1; j >= 0; j--)
                {
                    if (ris[j] > ris[min]) break;
                    min = j;
                }
                p.EdgeLeft = min + startIndex;

                min = max;
                for (int j = max + 1; j < ris.Length; j++)
                {
                    if (ris[j] > ris[min]) break;
                    min = j;
                }
                p.EdgeRight = min + startIndex;


                // 计算面积（斯托林斯基法、净峰面积）
                p.Area = Sterlinski(p, 5, 5);

                _peaks.Add(p);

                // 将i移位到当前峰的右边界，防止峰的重复判别
                i = min;
            }
            //*/
        }
        
        #endregion

        #region 峰面积

        /// <summary>
        /// 斯托林斯基峰面积计算
        /// </summary>
        /// <param name="p">需要计算的峰</param>
        /// <param name="k">参数k</param>
        /// <param name="l">参数l</param>
        /// <returns>净峰面积</returns>
        private double Sterlinski(Peak p, int k, int l)
        {
            // 如果峰位过大或过小，将无法完成计算，返回-1
            if (p.Position < k + l || p.Position > _points.Count - k - l - 1)
            {
                return -1;
            }

            double area = 0, areaK, bacK;

            for (int i = 0; i < l; i++)
            {
                areaK = bacK = 0;
                int start = p.Position - k - l + 1, end = p.Position + k + l - 1;
                // 总面积
                for (int j = start; j < end; j++)
                {
                    areaK += _points[j].Y;
                }

                start -= 1;
                end += 1;
                {   // 本底面积
                    bacK = (_points[start].Y + _points[end].Y) * (k + l - 0.5);
                }

                area += areaK - bacK;
            }

            return area;
        }

        #endregion

        #region Dialogs

        public async void ShowSmoothDialog()
        {
            await DialogHost.Show(new Views.SmoothInputView()
            {
                DataContext = IoC.Get<SmoothInputViewModel>(nameof(SmoothInputViewModel))
            }, "RootDialog");
            // _windowManager.ShowDialog(IoC.Get<SmoothInputViewModel>(nameof(SmoothInputViewModel)));
        }

        public async void ShowPeakDialog()
        {
            await DialogHost.Show(new Views.PeaksInputView()
            {
                DataContext = IoC.Get<PeaksInputViewModel>(nameof(PeaksInputViewModel))
            }, "RootDialog");
        }

        public void ShowSettingDialog()
        {
            _windowManager.ShowDialog(_container.GetInstance<SettingViewModel>(key: nameof(SettingViewModel)), null, null);
        }

        public async void ShowErrorDialog(string errorContent)
        {
            await DialogHost.Show(new Views.MessageView()
            {
                Message = { Text = errorContent }
            }, "RootDialog");
        }

        public async void ShowPopup(System.Windows.Controls.Button s)
        {
            await DialogHost.Show(new Views.MessageView()
            {
                Message = { Text = s.Content.ToString() }
            }, "RootDialog");
        }

        public void ShowMessage(string content)
        {
            _snackbarMessageQueue.Enqueue(content);
        }

        #endregion
    }
}
