using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CryptoMarketClient.ViewModels;

namespace CryptoMarketClient.Views;

public partial class ChartView : UserControl
{
    public ChartView()
    {
        InitializeComponent();
    }
    
    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ViewModel = (ChartViewModel)DataContext;
        SubscribeEvents(ViewModel);
    }

    public ChartViewModel ViewModel { get; private set; }

    private void SubscribeEvents(ChartViewModel viewModel)
    {
        if(viewModel != null)
        {
            viewModel.RequestCandleStickBestFit += ViewModelOnRequestCandleStickBestFit;
            viewModel.CandleStickDataChanged += ViewModelOnCandleStickDataChanged;
        }
    }

    private void ViewModelOnCandleStickDataChanged(object sender, CandleStickDataChangedEventArgs args)
    {
        UpdateChartSeries(args);
    }
    
    private bool _shouldUpdateVisualRange = true; 
    private void UpdateChartSeries(CandleStickDataChangedEventArgs args)
    {
        if (_shouldUpdateVisualRange)
        {
            _shouldUpdateVisualRange = false;
            Dispatcher.UIThread.Post(() => {
                UpdateCandleStickVisualRange();
                UpdateMarketDepthVisualRange();
                UpdateWallsVisualRange();
            });
        }
    }

    private void UpdateWallsVisualRange()
    {
    }

    private void UpdateMarketDepthVisualRange()
    {
    }

    private void UpdateCandleStickVisualRange()
    {
    }

    // protected void UpdateChartProperties() {
    //     ((BarSeriesView)this.chartControl1.Series["Volume"].View).BarWidth = 0.6 * Ticker.CandleStickPeriodMin;
    //     ((BarSeriesView)this.chartControl1.Series["Volume"].View).Border.Visibility = DevExpress.Utils.DefaultBoolean.False;
    //
    //     ((BarSeriesView)this.chartControl1.Series["BuySellVolume"].View).BarWidth = 0.6 * Ticker.CandleStickPeriodMin;
    //     ((BarSeriesView)this.chartControl1.Series["BuySellVolume"].View).Border.Visibility = DevExpress.Utils.DefaultBoolean.False;
    //
    //     FinancialSeriesViewBase f = this.chartControl1.Series["Current"].View as FinancialSeriesViewBase;
    //     if(f != null)
    //         f.LevelLineLength = 0.6 / 2 * Ticker.CandleStickPeriodMin;
    // }

    private int CalculateCandlestickBestFit()
    {
        double totalWidth = (int)ChartControl.Bounds.Width;
        double scale = 1.0; //LayoutHelper.GetLayoutScale(this);
        return (int)(totalWidth / (3 * scale));
    }
    
    void ViewModelOnRequestCandleStickBestFit(RequestCandleStickBestFitEventArgs obj)
    {
        obj.CandleStickCount = CalculateCandlestickBestFit();
    }
}