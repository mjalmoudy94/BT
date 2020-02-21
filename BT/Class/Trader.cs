using Binance.Net;
using Binance.Net.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using BT.Model;
using System.Net.Http;
using System.Timers;

namespace BT
{
    public static class Trader
    {
        public static List<BinanceStreamTick> PriceList = new List<BinanceStreamTick>();
        private static System.Timers.Timer aTimer;
        //
        //
        //
        public static Task BT = new Task(() =>
        {
            init();
        });
        //
        //
        //
        static void init()
        {
            //
            TBot.broadcastMessage("started");
            using (var client = new BinanceSocketClient())
            {
                client.SubscribeToSymbolTickerUpdatesAsync("BCHUSDT", (data) =>
                {
                    AddNewPrice(data);
                    tradeAnalyze();
                });

                // client.SubscribeToKlineUpdatesAsync("BCHUSDT", KlineInterval.OneMinute, (data) =>
                //{
                //    TBot.broadcastMessage(data.Data.Volume.ToString(new CultureInfo("en-US")));
                //});
            }
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(20000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += WakeIIsUpAsync;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        //
        static void WakeIIsUpAsync(Object source, ElapsedEventArgs e)
        {
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync("http://www.artacloud.ir");
        }
        //
        static void AddNewPrice(BinanceStreamTick data)
        {
            PriceList.Add(data);
        }
        //
        public static AnalyzeInfo MathAnalyze(int PeriodInSec)
        {
            AnalyzeInfo mathAnalyzeInfo = new AnalyzeInfo();
            //
            if (PriceList.Count > PeriodInSec)
            {
                var CuttedPriceList = PriceList.GetRange((int)(PriceList.Count - PeriodInSec - 1), (int)PeriodInSec);
                //
                mathAnalyzeInfo.CurrentPrice = PriceList.LastOrDefault().CurrentDayClosePrice;
                mathAnalyzeInfo.MaxPrice = CuttedPriceList.Max(p => p.CurrentDayClosePrice);
                mathAnalyzeInfo.MinPrice = CuttedPriceList.Min(p => p.CurrentDayClosePrice);
                mathAnalyzeInfo.AveragePrice = CuttedPriceList.Sum(p => p.CurrentDayClosePrice) / CuttedPriceList.Count;
                mathAnalyzeInfo.ChangeRange = mathAnalyzeInfo.MaxPrice - mathAnalyzeInfo.MinPrice;
                mathAnalyzeInfo.PeriodInSec = PeriodInSec;
            }
            //
            return mathAnalyzeInfo;
        }
        //
        //
        //
        public static decimal ValueInUSD = 100;
        public static decimal ValueInCrypto = 0.2M;
        static bool LockedOnSellPrice = false;
        static bool LockedOnBuyPrice = false;
        static void tradeAnalyze()
        {
            //AnalyzeInfo infoOf3600 = MathAnalyze(3600);
            AnalyzeInfo infoOf1800 = MathAnalyze(3600);
            TBot.broadcastMessage("NEW PRICE", infoOf1800, 1);
            //
            //
            if (!LockedOnBuyPrice &&
                ValueInUSD > 11 &&
                (infoOf1800.MaxPrice - infoOf1800.MinPrice) > 6 &&
                infoOf1800.CurrentPrice == infoOf1800.MinPrice)
            {
                LockedOnBuyPrice = true;
                TBot.broadcastMessage("Lock On Buy");
            }

            if (!LockedOnSellPrice &&
                (infoOf1800.CurrentPrice * ValueInCrypto) > 11 &&
                (infoOf1800.MaxPrice - infoOf1800.MinPrice) > 6 &&
                infoOf1800.CurrentPrice == infoOf1800.MaxPrice)
            {
                LockedOnSellPrice = true;
                TBot.broadcastMessage("Lock On Sell");
            }

            if (LockedOnBuyPrice)
            {
                TBot.broadcastMessage("Buying", infoOf1800, 1);
                if (infoOf1800.CurrentPrice - infoOf1800.MinPrice > 2)
                {
                    ValueInCrypto += ((ValueInUSD / 2) / infoOf1800.CurrentPrice);
                    ValueInUSD -= (ValueInUSD / 2);
                    LockedOnBuyPrice = false;
                    //
                    TBot.broadcastMessage("Buy: ₿" + (float)(ValueInUSD), infoOf1800, 0);
                }
            }

            if (LockedOnSellPrice)
            {
                TBot.broadcastMessage("Selling", infoOf1800, 1);
                if ((infoOf1800.MaxPrice - infoOf1800.CurrentPrice) > 2)
                {
                    ValueInUSD += ((ValueInCrypto / 2) * infoOf1800.CurrentPrice);
                    ValueInCrypto -= ValueInCrypto / 2;
                    LockedOnSellPrice = false;
                    //
                    TBot.broadcastMessage("Sell: $" + (float)(ValueInCrypto), infoOf1800, 0);
                }
            }

        }

    }
}