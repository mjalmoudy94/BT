using Binance.Net;
using Binance.Net.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;

namespace BT
{
    public static class Trader
    {

        //
        public static Task BT = new Task(() =>
        {
            init();
            //
            while (true)
            {
                //TBot.broadcastMessage("test");
                System.Threading.Thread.Sleep(5000);
            }
        });
        //
        static void init()
        {
            //
            using (var client = new BinanceSocketClient())
            {
                client.SubscribeToSymbolTickerUpdatesAsync("BCHUSDT", (data) =>
                {
                    TickerUpdate(data);
                });

                client.SubscribeToKlineUpdatesAsync("BCHUSDT", KlineInterval.OneMinute , (data) =>
                {
                    KlineUpdate(data);
                });
            }
        }
        //
        //
        // New Price Change Update
        static void TickerUpdate(BinanceStreamTick data)
        {
            TBot.broadcastMessage(data.WeightedAverage.ToString("C", new CultureInfo("en-US")));
        }
        //
        static void KlineUpdate(BinanceStreamKlineData data)
        {
            TBot.broadcastMessage(data.Data.Volume.ToString(new CultureInfo("en-US")));
        }
        //

    }
}