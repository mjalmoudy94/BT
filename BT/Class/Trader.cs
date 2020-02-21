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
        static  void WakeIIsUpAsync(Object source, ElapsedEventArgs e)
        {
            HttpClient client = new HttpClient();
            var response = client.GetStringAsync("http://www.artacloud.ir");
        }
        //
        static void AddNewPrice(BinanceStreamTick data)
        {
            PriceList.Add(data);
            //
            if (PriceList.Count > 36000)
            {
                PriceList.RemoveAt(0);
            }
            //
            if ((PriceList.Count % 1000) == 0)
            {
                TBot.broadcastMessage("PriceList Count :" + PriceList.Count);
            }
        }
        //
        static AnalyzeInfo MathAnalyze(int PeriodInSec)
        {
            AnalyzeInfo mathAnalyzeInfo = new AnalyzeInfo();
            //
            if (PriceList.Count > PeriodInSec)
            {
                var CuttedPriceList = PriceList.GetRange((int)(PriceList.Count - PeriodInSec), (int)PeriodInSec - 1);
                //
                mathAnalyzeInfo.CurrentPrice = PriceList.LastOrDefault().CurrentDayClosePrice;
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
        static decimal ValueInUSD = 0;
        static decimal ValueInCrypto = 1;
        static decimal LockedPrice = 0;
        static decimal LastTradePrice = 0;
        static void tradeAnalyze()
        {
            //AnalyzeInfo infoOf3600 = MathAnalyze(3600);
            AnalyzeInfo infoOf1800 = MathAnalyze(1800);

            if (ValueInUSD < (ValueInCrypto * infoOf1800.CurrentPrice))
            {
                // start to lock on sell
                if (LockedPrice == 0 &&
                    infoOf1800.CurrentPrice - LastTradePrice > 10 &&
                    (infoOf1800.MaxPrice < infoOf1800.CurrentPrice))
                {
                    TBot.broadcastMessage("start lock on sell: $" + infoOf1800.CurrentPrice);
                    LockedPrice = infoOf1800.CurrentPrice;
                }

                // update lock on sell
                if (LockedPrice > 0 &&
                    infoOf1800.CurrentPrice > LockedPrice)
                {
                    TBot.broadcastMessage("update lock on sell: $" + infoOf1800.CurrentPrice);
                    LockedPrice = infoOf1800.CurrentPrice;
                }

                // sell
                if (LockedPrice > 0 &&
                    (LockedPrice - infoOf1800.CurrentPrice) > 3)
                {
                    TBot.broadcastMessage("sell On: $" + infoOf1800.CurrentPrice);
                    LockedPrice = 0;
                    ValueInUSD = ValueInCrypto * infoOf1800.CurrentPrice;
                    ValueInCrypto = 0;
                    
                }

            }
            else
            {
                // start to lock on buy
                if (LockedPrice == 0 &&
                    LastTradePrice - infoOf1800.CurrentPrice > 10 &&
                    (infoOf1800.MinPrice > infoOf1800.CurrentPrice))
                {
                    TBot.broadcastMessage("start lock on buy: $" + infoOf1800.CurrentPrice);
                    LockedPrice = infoOf1800.CurrentPrice;
                }

                // update lock on buy
                if (LockedPrice > 0 &&
                    infoOf1800.CurrentPrice < LockedPrice)
                {
                    TBot.broadcastMessage("update lock on buy: $" + infoOf1800.CurrentPrice);
                    LockedPrice = infoOf1800.CurrentPrice;
                }

                // buy
                if (LockedPrice > 0 &&
                    (infoOf1800.CurrentPrice - LockedPrice) > 3)
                {
                    TBot.broadcastMessage("sell On: $" + infoOf1800.CurrentPrice);
                    LockedPrice = 0;
                    ValueInCrypto = ValueInUSD / infoOf1800.CurrentPrice;
                    ValueInUSD = 0;
                }
            }

        }
    }
}