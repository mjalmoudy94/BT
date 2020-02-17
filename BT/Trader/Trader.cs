using Binance.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BT
{
    public static class Trader
    {

        //
        public static Task BT = new Task(() => {
            while (true)
            {

                System.Threading.Thread.Sleep(1000);
            }
        });
        //
        static void init()
        {
			using (var client = new BinanceSocketClient())
			{
				var successDepth = client.SubscribeToDepthStream("bnbbtc", (data) =>
				{
					// handle data
				});
				var successTrades = client.SubscribeToTradesStream("bnbbtc", (data) =>
				{
					// handle data
				});
				var successKline = client.SubscribeToKlineStream("bnbbtc", KlineInterval.OneMinute, (data) =>
				{
					// handle data
				});
				var successSymbol = client.SubscribeToSymbolTicker("bnbbtc", (data) =>
				{
					// handle data
				});
				var successSymbols = client.SubscribeToAllSymbolTicker((data) =>
				{
					// handle data
				});
				var successOrderBook = client.SubscribeToPartialBookDepthStream("bnbbtc", 10, (data) =>
				{
					// handle data
				});
			}
		}
    }
}