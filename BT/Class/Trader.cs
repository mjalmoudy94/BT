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
                    tradeAnalyze(data);
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
            mathAnalyzeInfo.CurrentPrice = PriceList.LastOrDefault().CurrentDayClosePrice;
            mathAnalyzeInfo.PeriodInSec = PeriodInSec;
            //
            if (PriceList.Count > PeriodInSec)
            {
                var CuttedPriceList = PriceList.GetRange((int)(PriceList.Count - PeriodInSec - 1), (int)PeriodInSec);
                //
                mathAnalyzeInfo.MaxPrice = CuttedPriceList.Max(p => p.CurrentDayClosePrice);
                mathAnalyzeInfo.MinPrice = CuttedPriceList.Min(p => p.CurrentDayClosePrice);
                mathAnalyzeInfo.AveragePrice = CuttedPriceList.Sum(p => p.CurrentDayClosePrice) / CuttedPriceList.Count;
                mathAnalyzeInfo.ChangeRange = mathAnalyzeInfo.MaxPrice - mathAnalyzeInfo.MinPrice;
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
        static decimal LockedSellPrice = 0;
        static decimal LastSellPrice = 0;
        static bool LockedOnBuyPrice = false;
        static decimal LockedBuyPrice = 0;
        static decimal LastBuyPrice = 0;
        static void tradeAnalyze(BinanceStreamTick data)
        {
            if (LastSellPrice == 0)
            {
                LastSellPrice = data.CurrentDayClosePrice;
                LastBuyPrice = data.CurrentDayClosePrice;
            }

            //AnalyzeInfo infoOf3600 = MathAnalyze(3600);
            AnalyzeInfo infoOf1800 = MathAnalyze(3600 * 6);
            TBot.broadcastMessage("NEW PRICE :" + (int)data.CurrentDayClosePrice, infoOf1800, 2);
            //
            //
            if (!LockedOnBuyPrice &&
                ValueInUSD > 11 &&
                (LastSellPrice - data.CurrentDayClosePrice) > 10)
            {
                LockedOnBuyPrice = true;
                LockedBuyPrice = data.CurrentDayClosePrice;
                TBot.broadcastMessage("Lock On Buy\n", infoOf1800, 0);
            }

            if (!LockedOnSellPrice &&
                (data.CurrentDayClosePrice * ValueInCrypto) > 11 &&
                (data.CurrentDayClosePrice - LastBuyPrice) > 10)
            {
                LockedOnSellPrice = true;
                LockedSellPrice = data.CurrentDayClosePrice;
                TBot.broadcastMessage("Lock On Sell\n", infoOf1800, 0);
            }

            if (LockedOnBuyPrice)
            {
                TBot.broadcastMessage("Locked in buy: $" + LockedBuyPrice + "\n", infoOf1800, 1);
                if ((data.CurrentDayClosePrice - LockedBuyPrice) > 2)
                {
                    ValueInCrypto += ((ValueInUSD / 2) / data.CurrentDayClosePrice);
                    ValueInUSD -= (ValueInUSD / 2);
                    LockedOnBuyPrice = false;
                    LastBuyPrice = LockedBuyPrice;
                    LockedBuyPrice = 0;
                    //
                    TBot.broadcastMessage("Buy: $" + (float)(ValueInUSD), infoOf1800, 0);
                }
            }

            if (LockedOnSellPrice)
            {
                TBot.broadcastMessage("Locked in Sell: " + LockedSellPrice + "\n", infoOf1800, 1);
                if ((LockedSellPrice - data.CurrentDayClosePrice) > 2)
                {
                    ValueInUSD += ((ValueInCrypto / 2) * data.CurrentDayClosePrice);
                    ValueInCrypto -= ValueInCrypto / 2;
                    LastSellPrice = LockedSellPrice;
                    LockedSellPrice = 0;
                    LockedOnSellPrice = false;
                    //
                    TBot.broadcastMessage("Sell: $" + (float)(ValueInCrypto * data.CurrentDayClosePrice), infoOf1800, 0);
                }
            }

        }

    }
}