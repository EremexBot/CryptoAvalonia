using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Crypto.Core;
using CryptoMarketClient.Utils;
using CryptoMarketClient.ViewModels;
using Eremex.AvaloniaUI.Controls.Common;
using Eremex.AvaloniaUI.Controls.DataControl.Visuals;
using Eremex.AvaloniaUI.Controls.DataGrid.Visuals;

namespace CryptoMarketClient.Views;

public partial class ExchangeView : UserControl, IExchangeView
{
    public ExchangeView()
    {
        InitializeComponent();
    }

    private MxVirtualizingControl _virtualizingControl;
    private ScrollViewer _scrollViewer;
    protected ExchangeViewModel ViewModel { get; set; }
    protected override void OnDataContextChanged(EventArgs e)
    {
        if(ViewModel != null)
        {
            ViewModel.OnDetached();
            UnsubscribeEvents(ViewModel);
        }

        base.OnDataContextChanged(e);
        ExchangeViewModel vm = (ExchangeViewModel)DataContext;
        if(vm != null)
        {
            vm.OnAttached(this);
            SubscribeEvents(vm);
        }

        ViewModel = vm;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        TickersGrid.TemplateApplied -= TickersGridOnTemplateApplied;
        TickersGrid.TemplateApplied += TickersGridOnTemplateApplied;
    }

    private void TickersGridOnTemplateApplied(object sender, TemplateAppliedEventArgs e)
    {
        if(_scrollViewer != null)
            _scrollViewer.ScrollChanged -= ScrollViewerOnScrollChanged;
        _scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollViewer");
        _virtualizingControl = e.NameScope.Find<MxVirtualizingControl>("PART_VirtualizingControl");
        if(_scrollViewer != null)
            _scrollViewer.ScrollChanged += ScrollViewerOnScrollChanged;
        UpdateVisibleItems();
    }

    private void ScrollViewerOnScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(UpdateVisibleItems, DispatcherPriority.Render);
    }

    private List<object> _visibleItems;
    private void UpdateVisibleItems()
    {
         if(_scrollViewer == null)
             return;
         _visibleItems = new List<object>();
         List<DataGridRowControl> rows = new List<DataGridRowControl>(); 
         foreach(var c in _virtualizingControl.GetContainers(false)) {
             if(c is DataGridRowControl dg)
                rows.Add(dg);
         }
        _visibleItems = rows.OrderBy(it => it.Bounds.Y).Select(it => ((DataGridRowControl)it).Row).ToList();
    }

    private void UnsubscribeEvents(ExchangeViewModel viewModel)
    {
        viewModel.RequestUpdateData -= OnUpdateData;
        viewModel.RequestVisibleTickers -= OnRequestVisibleTickers;
    }

    private void OnRequestVisibleTickers(object sender, RequestVisibleItemsEventArgs e)
    {
        if(_visibleItems == null || _visibleItems.Count == 0)
        {
            Dispatcher.UIThread.Invoke(UpdateVisibleItems);
        }
        e.VisibleItems = _visibleItems;
    }

    private void SubscribeEvents(ExchangeViewModel viewModel)
    {
        viewModel.RequestUpdateData += OnUpdateData;
        viewModel.RequestVisibleTickers += OnRequestVisibleTickers;
        viewModel.RequestUpdateTicker += OnRequestUpdateTicker;
    }

    private void OnRequestUpdateTicker(TickerUpdateEventArgs obj)
    {
        
    }

    public void OnUpdateData()
    {
        Dispatcher.UIThread.Post(() =>
        {
            TickersGrid.RefreshData();    
        });
    }

    public void OnUpdateTicker(TickerUpdateEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var rowIndex = TickersGrid.GetRowIndexBySourceItemIndex(e.Ticker.Exchange.Tickers.IndexOf(e.Ticker));
            TickersGrid.RefreshRow(rowIndex);    
        });
    }

    private void TickersGrid_OnDoubleTapped(object sender, TappedEventArgs e)
    {
        ViewModel?.OpenTicker(ViewModel.SelectedTicker);
    }
}