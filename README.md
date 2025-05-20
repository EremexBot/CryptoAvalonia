## How to build and run
```
git clone https://github.com/EremexBot/CryptoAvalonia
cd CryptoAvalonia
git submodule update --init
cd CryptoMarketClient.Avalonia
dotnet run
```

## License
CryptoTradingFramework © 2025 by Abazian Arsen. The code in this repo is available under the CC BY-NC 4.0 license, which allows for downloading, sharing, printing, multiplication, and use for any non-commercial purpose by the authors and others.

## About 

This repository contains an Avalonia-based framework and client application for cryptocurrency exchange trading, supporting both manual and automated strategies.

![readme-client](/images/readme-client.png)

![readme-grid-flat-keys](/images/readme-grid-flat-keys.png)

![readme-grid-flat-list](/images/readme-grid-flat-list.png)

![readme-chart-crosshair](/images/readme-chart-crosshair.png)

![readme-mdi-floatingpanel](/images/readme-mdi-floatingpanel.png)



![CryptoTraderAuto](https://github.com/ArsenAbazian/CryptoTradingFramework/blob/master/Help/ExchangesForm.png)
![CryptoTraderAuto](https://user-images.githubusercontent.com/18391055/186918434-a3970be3-173b-47f3-bf8e-d3f96dd09189.png)
![CryptoTraderAuto](https://github.com/ArsenAbazian/CryptoTradingFramework/blob/master/Help/CryptoTraderAuto-TickersScreen.png)
![CryptoTraderAuto](https://github.com/ArsenAbazian/CryptoTradingFramework/blob/master/Help/WhatsNew_01_25_2022.png)
![CryptoTraderAuto](https://github.com/ArsenAbazian/CryptoTradingFramework/blob/master/Help/CryptoTraderAuto-AtGlance.png)

The CryptoTradingFramework library allows you to create custom applications for cryptocurrency exchanges, supporting the following features:

* Monitoring
* Backtesting
* Analysis
* Trading Bots
* Classic Arbitrage
* Manual Trading
* etc

Supported exchanges (in active development):
* Poloniex
* Bittrex
* Binance
* Binance futures (partially)
* Bitmex
* Kraken
* BitFinex

The `CryptoMarketClient` Avalonia application supports both manual trading and automated trading bot operations with various strategies. It features classic arbitrage monitoring, statistical arbitrage monitoring, and supports multiple trading strategies.

The `CryptoMarketClient` app utilizes the [EMX Controls library](https://www.nuget.org/packages/Eremex.Avalonia.Controls) for Avalonia to build its UI. The application makes extensive use of these EMX components:
- DataGridControl — Displays data from an item source as a table. Supports built-in data sorting, grouping, filtering, searching, and editing functionality.
- CartesianChart — Allows you to plot most popular charts: Candlestick, Line, Bar, Range Bar, and more.
- DockManager — Allows you to build and manage Microsoft Visual Studio-inspired docking interfaces.

<!-- PoloniexClient allows you to grab and save historical data from exchanges and then apply them for strategies simulation. **This application is under development.** -->

_This repository is an Avalonia-based port of the [CryptoTradingFramework](https://github.com/ArsenAbazian/CryptoTradingFramework) repository written in WinForms. The current repository is under active development._


If you'd like to thank me and help fuel future development, cryptocurrency donations are warmly appreciated:

* **BTC**     1PjFCsfVYwhU8CUqTKAGD2UdTeKqgemb1R
* **ETH** 	  0xa709fc01c5d32500f1c18605d48a7b7bd03f5893
* **XRP**     rEb8TK3gBgk5auZkwc6sHnwrGVJH8DuaLh            Tag = 106961926
* **LTC** 	  LPKGsLjnpc3xRsvPYYHuXhpFpx6zhpVg9n 

### **Roadmap** 
Currently I am working on adding LOW-CODE platform support for customizing strategies. Please see https://github.com/ArsenAbazian/WorkflowDiagram for greater details.




## Latest Updates 

[Update From 11-09-2022](https://github.com/ArsenAbazian/CryptoTradingFramework/wiki/Update-From-11-09-2022)

[Update From 23-03-2022](https://github.com/ArsenAbazian/CryptoTradingFramework/wiki/Update-From-23-03-2022)

[Update From 24-02-2022](https://github.com/ArsenAbazian/CryptoTradingFramework/wiki/Update-From-24-02-2022)

[Update From 09-02-2022](https://github.com/ArsenAbazian/CryptoTradingFramework/wiki/Update-From-09-02-2022)

[Update From 25-01-2022](https://github.com/ArsenAbazian/CryptoTradingFramework/wiki/Update-From-01-25-2022)