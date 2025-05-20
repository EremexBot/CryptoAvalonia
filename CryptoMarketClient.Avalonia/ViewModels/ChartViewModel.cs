using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Crypto.Core;
using Crypto.Core.Helpers;
using CryptoMarketClient.Utils;
using CryptoMarketClient.ViewModels.Actions;
using Eremex.AvaloniaUI.Charts;

namespace CryptoMarketClient.ViewModels;

public partial class ChartViewModel : ViewModelBase
{
    [ObservableProperty] private Ticker ticker;
    [ObservableProperty] private TickerCandlestickDataAdapter price;
    [ObservableProperty] private TickerValueDataAdapter volume;
    [ObservableProperty] private TickerValueDataAdapter buy;
    [ObservableProperty] private TickerValueDataAdapter sell;
    [ObservableProperty] private TickerValueDataAdapter events;
    [ObservableProperty] private bool supportBuySellVolume;
    [ObservableProperty] private double barWidth;
    [ObservableProperty] private DateTimeUnit measureUnit = DateTimeUnit.Minute;
    [ObservableProperty] private int measureUnitFactor = 30;
    [ObservableProperty] private string currentPeriod;
    [ObservableProperty] private ObservableCollection<ToolbarCheckItemViewModel> _periodItems = new();

    public ChartViewModel(DocumentManager documentManager, IToolbarController toolbarController, Ticker ticker) : base(
        documentManager, toolbarController)
    {
        this.ticker = ticker;
        price = new TickerCandlestickDataAdapter(Ticker);
        volume = new TickerValueDataAdapter(Ticker, TickerVolumeType.Volume);
        buy = new TickerValueDataAdapter(Ticker, TickerVolumeType.BuyVolume);
        sell = new TickerValueDataAdapter(Ticker, TickerVolumeType.SellVolume);
        events = new TickerValueDataAdapter(Ticker, TickerVolumeType.Event);
    }

    protected override ToolbarManagerViewModel CreateToolbars()
    {
        return null;
    }

    private void EventsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateEvents(e);
    }

    private void UpdateEvents(NotifyCollectionChangedEventArgs e)
    {
        if (e == null || e.Action == NotifyCollectionChangedAction.Reset)
        {
            Events.OnDataChanged(new SeriesDataAdapterDataChangedEventArgs(SeriesDataUpdateType.Reset, null));
            return;
        }

        SeriesDataUpdateType type = SeriesDataUpdateType.Reset;
        List<int> items = null;
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            type = SeriesDataUpdateType.ItemsAdded;
            items = new List<int>(e.NewItems.Count);
            for (int i = e.NewStartingIndex; i < e.NewStartingIndex + e.NewItems.Count; i++)
            {
                items.Add(i);
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            type = SeriesDataUpdateType.ItemsRemoved;
            items = new List<int>(e.NewItems.Count);
            for (int i = e.OldStartingIndex; i < e.OldStartingIndex + e.OldItems.Count; i++)
            {
                items.Add(i);
            }
        }

        var args = new SeriesDataAdapterDataChangedEventArgs(type, items);
        Events.OnDataChanged(args);
    }

    private void OnTickerCandleStickChanged(object sender, ListChangedEventArgs e)
    {
        SeriesDataAdapterDataChangedEventArgs ee = null;
        if (e.ListChangedType == ListChangedType.ItemAdded)
        {
            ee =
                new SeriesDataAdapterDataChangedEventArgs(SeriesDataUpdateType.ItemsAdded,
                    new List<int>(1) { e.NewIndex });
        }
        else if (e.ListChangedType == ListChangedType.ItemChanged)
        {
            ee =
                new SeriesDataAdapterDataChangedEventArgs(SeriesDataUpdateType.ItemsChanged,
                    new List<int>(1) { e.NewIndex });
        }

        if (ee == null)
            return;
        Price.OnDataChanged(ee);
        Volume.OnDataChanged(ee);
        Buy.OnDataChanged(ee);
        Sell.OnDataChanged(ee);
    }

    public override void OnDetached()
    {
        base.OnDetached();
        Ticker.CandleStickChanged -= OnTickerCandleStickChanged;
        Ticker.EventsChanged -= EventsChanged;
        Ticker.StopListenKlineStream();
    }

    public override void OnAttached(object view)
    {
        base.OnAttached(view);
        Ticker.CandleStickChanged += OnTickerCandleStickChanged;
        Ticker.EventsChanged += EventsChanged;
        Ticker.StopListenKlineStream();
        SetCandleStickCheckItemValues();
        UpdateChartViewSettings();
        UpdateCandleStickMenu();
        UpdateDataDromServerAsync().ContinueWith(t =>
        {
            if (Ticker.Exchange.SupportWebSocket(WebSocketType.Kline))
                Ticker.StartListenKlineStream();
        });
    }

    void UpdateCandleStickMenu()
    {
        foreach (ToolbarCheckItemViewModel item in _periodItems)
        {
            if (((TimeSpan)item.Tag).Minutes == Ticker.CandleStickPeriodMin)
            {
                item.IsChecked = true;
                CurrentPeriod = item.Header;
                break;
            }
        }
    }

    private bool _suppressUpdateCandlestickData;

    protected void UpdateChartViewSettings()
    {
        _suppressUpdateCandlestickData = true;
        try
        {
            SupportBuySellVolume = Ticker.Exchange.SupportBuySellVolume;
            var intervals = Ticker.Exchange.AllowedCandleStickIntervals;
            if (Ticker.CandleStickPeriodMin < 60)
            {
                MeasureUnit = DateTimeUnit.Minute;
                MeasureUnitFactor = Ticker.CandleStickPeriodMin;
            }
            else if (Ticker.CandleStickPeriodMin < 24 * 60)
            {
                MeasureUnit = DateTimeUnit.Hour;
                MeasureUnitFactor = Ticker.CandleStickPeriodMin / 60;
            }
            else
            {
                MeasureUnit = DateTimeUnit.Day;
                MeasureUnitFactor = Ticker.CandleStickPeriodMin / (24 * 60);
            }

            //this.chartControl1.Series["Volume"].ArgumentDataMember = "Time";
            //this.chartControl1.Series["Volume"].ValueDataMembers.AddRange("Volume");

            //this.chartControl1.Series["Events"].DataSource = Ticker.Events;
            //this.chartControl1.Series["Events"].ArgumentDataMember = "Time";
            //this.chartControl1.Series["Events"].ValueDataMembers.AddRange("Current");

            // this.chartMarketDepth.Series["TotalVolumeBuy"].DataSource = Ticker.OrderBook.Bids;
            // this.chartMarketDepth.Series["TotalVolumeBuy"].ArgumentDataMember = "Value";
            // this.chartMarketDepth.Series["TotalVolumeBuy"].ValueDataMembers.AddRange("VolumeTotal");
            //
            // this.chartMarketDepth.Series["TotalVolumeSell"].DataSource = Ticker.OrderBook.Asks;
            // this.chartMarketDepth.Series["TotalVolumeSell"].ArgumentDataMember = "Value";
            // this.chartMarketDepth.Series["TotalVolumeSell"].ValueDataMembers.AddRange("VolumeTotal");
            //
            // this.chartWalls.Series["Sell volume"].DataSource = Ticker.OrderBook.Asks;
            // this.chartWalls.Series["Sell volume"].ArgumentDataMember = "Value";
            // this.chartWalls.Series["Sell volume"].ValueDataMembers.AddRange("Volume");
            // this.chartWalls.Series["Sell volume"].View.Color = Exchange.AskColor;
            //
            // this.chartWalls.Series["Buy volume"].DataSource = Ticker.OrderBook.Bids;
            // this.chartWalls.Series["Buy volume"].ArgumentDataMember = "Value";
            // this.chartWalls.Series["Buy volume"].ValueDataMembers.AddRange("Volume");
            // this.chartWalls.Series["Buy volume"].View.Color = Exchange.BidColor;

            UpdateEvents(null);
        }
        finally
        {
            _suppressUpdateCandlestickData = false;
        }
    }

    [RelayCommand]
    private async Task RefreshChart()
    {
        Ticker.StopListenKlineStream();
        Ticker.CandleStickData.Clear();
        Ticker.StartListenKlineStream();

        await UpdateDataDromServerAsync();
    }
    
    private void SetCandleStickCheckItemValues()
    {
        var intervals = Ticker.Exchange.AllowedCandleStickIntervals;
        for (int i = 0; i < intervals.Count; i++)
        {
            CandleStickIntervalInfo info = intervals[i];
            ToolbarCheckItemViewModel item = new ToolbarCheckItemViewModel()
                { Header = info.Text, Tag = info.Interval };
            PeriodItems.Add(item);
            item.PropertyChanged += OnIntervalItemPropertyChanged;
        }
    }

    private async void OnIntervalItemPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        ToolbarCheckItemViewModel checkItem = (ToolbarCheckItemViewModel)sender;
        if (e.PropertyName == nameof(ToolbarCheckItemViewModel.IsChecked) && checkItem.IsChecked)
        {
            if ((int)((TimeSpan)checkItem.Tag).TotalMinutes == Ticker.CandleStickPeriodMin)
                return;
            CurrentPeriod = checkItem.Header;
            Ticker.StopListenKlineStream();
            Ticker.CandleStickPeriodMin = (int)((TimeSpan)checkItem.Tag).TotalMinutes;
            Ticker.CandleStickData.Clear();
            Ticker.StartListenKlineStream();

            await UpdateDataDromServerAsync();
        }
    }

    private async Task UpdateDataDromServerAsync()
    {
        if (_isCandleSticksUpdate)
            return;

        DateTime start = DateTime.UtcNow; //.Subtract(TimeSpan.FromSeconds(totalInterval));
        await UpdateCandleSticksAsync(start).ContinueWith(t => { UpdateChartProperties(); });
    }

    private void UpdateChartProperties()
    {
        BarWidth = 0.6 * Ticker.CandleStickPeriodMin;
    }

    private bool _isCandleSticksUpdate;

    public event Action<RequestCandleStickBestFitEventArgs> RequestCandleStickBestFit;
    public event Action<object, CandleStickDataChangedEventArgs> CandleStickDataChanged;

    private int CalculateTotalIntervalInSeconds()
    {
        int screenCount = 3;
        var args = new RequestCandleStickBestFitEventArgs() { CandleStickCount = 300 };
        RequestCandleStickBestFit?.Invoke(args);
        int candleStickCount = args.CandleStickCount;
        int interval = Ticker.CandleStickPeriodMin * 60;
        return candleStickCount * screenCount * interval;
    }

    private async Task UpdateCandleSticksAsync(DateTime date)
    {
        if (_isCandleSticksUpdate)
            return;
        _isCandleSticksUpdate = true;

        await Task.Run(() =>
        {
            int seconds = CalculateTotalIntervalInSeconds();
            //ResizeableArray<CandleStickData> data;
            // if (date.Date == DateTime.UtcNow.Date)
            //     data = Ticker.GetRecentCandleStickData(Ticker.CandleStickPeriodMin);
            // else
            var data = Ticker.GetCandleStickData(Ticker.CandleStickPeriodMin, date.AddSeconds(-seconds), seconds);
            if (data != null)
            {
                _isCandleSticksUpdate = false;
                Ticker.CandleStickData.Insert(0, data);
                SeriesDataAdapterDataChangedEventArgs e =
                    new SeriesDataAdapterDataChangedEventArgs(SeriesDataUpdateType.Reset, null);
                Price.OnDataChanged(e);
                Volume.OnDataChanged(e);
                Buy.OnDataChanged(e);
                Sell.OnDataChanged(e);
                CandleStickDataChanged?.Invoke(this, new CandleStickDataChangedEventArgs() { Data = data });
            }
        });
    }
}

public class RequestCandleStickBestFitEventArgs : EventArgs
{
    public int CandleStickCount { get; set; }
}

public class CandleStickDataChangedEventArgs : EventArgs
{
    public ResizeableArray<CandleStickData> Data { get; internal set; }
}

public class TickerDataAdapterBase : ISeriesDataAdapter
{
    public TickerDataAdapterBase(Ticker ticker)
    {
        Ticker = ticker;
        _scaleTypes.Add(AxisType.Argument, ScaleType.DateTime);
        _scaleTypes.Add(AxisType.Value, ScaleType.Numeric);
    }

    protected Ticker Ticker { get; set; }
    private readonly Dictionary<AxisType, ScaleType> _scaleTypes = new();
    public Dictionary<AxisType, ScaleType> GetScaleTypes() => _scaleTypes;

    public virtual double GetNumericalValue(int index, SeriesDataMemberType dataMember)
    {
        return 0;
    }

    public virtual DateTime GetDateTimeValue(int index, SeriesDataMemberType dataMember)
    {
        return DateTime.Now;
    }

    public virtual TimeSpan GetTimeSpanValue(int index, SeriesDataMemberType dataMember)
    {
        throw new NotImplementedException();
    }

    public virtual string GetQualitativeValue(int index, SeriesDataMemberType dataMember)
    {
        throw new NotImplementedException();
    }

    public virtual string GetUnderlyingData(int index)
    {
        throw new NotImplementedException();
    }

    public virtual int ItemCount => 0;
    public bool IsDataSorted => true;
    public event SeriesDataAdapterDataChangedEventHandler DataChanged;

    public void OnDataChanged(SeriesDataAdapterDataChangedEventArgs e)
    {
        if (Dispatcher.UIThread.CheckAccess())
            DataChanged?.Invoke(this, e);
        else
        {
            Dispatcher.UIThread.Post(() => { DataChanged?.Invoke(this, e); });
        }
    }
}

public class TickerCandlestickDataAdapter(Ticker ticker) : TickerDataAdapterBase(ticker)
{
    public override double GetNumericalValue(int index, SeriesDataMemberType dataMember)
    {
        var cs = Ticker.CandleStickData[index];
        if (cs == null)
            return 0;
        if (dataMember == SeriesDataMemberType.Open)
            return cs.Open;
        if (dataMember == SeriesDataMemberType.Close)
            return cs.Close;
        if (dataMember == SeriesDataMemberType.High)
            return cs.High;
        if (dataMember == SeriesDataMemberType.Low)
            return cs.Low;
        return 0;
    }

    public override int ItemCount => Ticker.CandleStickData.Count;

    public override DateTime GetDateTimeValue(int index, SeriesDataMemberType dataMember)
    {
        var d = Ticker.CandleStickData[index];
        if (d == null)
            return DateTime.Now;
        return Ticker.CandleStickData[index].Time;
    }

    public override string GetUnderlyingData(int index)
    {
        if (index >= Ticker.CandleStickData.Count)
            return "";
        return Ticker.CandleStickData[index].High.ToString(CultureInfo.InvariantCulture);
    }
}

public enum TickerVolumeType
{
    Volume,
    BuyVolume,
    SellVolume,
    Event
}

public class TickerValueDataAdapter(Ticker ticker, TickerVolumeType type) : TickerDataAdapterBase(ticker)
{
    private TickerVolumeType Type { get; set; } = type;

    public override double GetNumericalValue(int index, SeriesDataMemberType dataMember)
    {
        if (Type == TickerVolumeType.Event)
        {
            var ev = Ticker.Events[index];
            return ev?.Current ?? 0;
        }

        var cs = Ticker.CandleStickData[index];
        if (cs == null)
            return 0;
        if (Type == TickerVolumeType.Volume)
            return cs.Volume;
        if (Type == TickerVolumeType.BuyVolume)
            return cs.BuyVolume;
        return cs.SellVolume;
    }

    public override DateTime GetDateTimeValue(int index, SeriesDataMemberType dataMember)
    {
        if (Type == TickerVolumeType.Event)
        {
            var ev = Ticker.Events[index];
            return ev?.Time ?? DateTime.Now;
        }

        var cs = Ticker.CandleStickData[index];
        return cs?.Time ?? DateTime.Now;
    }

    public override int ItemCount =>
        Type == TickerVolumeType.Event ? Ticker.Events.Count : Ticker.CandleStickData.Count;

    public override string GetUnderlyingData(int index)
    {
        if (Type == TickerVolumeType.Event)
        {
            if (index >= Ticker.Events.Count)
                return "";
            return Ticker.Events[index].Text;
        }

        if (index >= Ticker.CandleStickData.Count)
            return "";
        return Ticker.CandleStickData[index].High.ToString(CultureInfo.InvariantCulture);
    }
}