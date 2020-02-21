﻿using BT.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BT
{
    public static class TBot
    {
        private static List<string> UsersID = new List<string>
        {
            "78649634"
            ,"485736315"
        };
        private static string Token = "992031045:AAEYQvxujIKOlMHWesz_JzoYKCBzq1UoXh4";
        //
        public static TelegramBotClient telegram = new TelegramBotClient(Token);
        //
        //
        //
        static decimal lastBroudCastPrice = 0;
        public static void broadcastMessage(string MSG, AnalyzeInfo info = null, decimal ChangeRange = 0)
        {
            StringBuilder Message = new StringBuilder();
            Message.Append(MSG + "\n");
            //
            if (info != null)
            {
                Message.Append(info.PeriodInSec / 60 + " minute's\n");
                Message.Append("USDT:" + (float)Trader.ValueInUSD + "\n");
                Message.Append("Crypto:" + (float)Trader.ValueInCrypto + "\n");
                Message.Append("value of wallet:" + (float)(Trader.ValueInUSD + (Trader.ValueInCrypto * info.CurrentPrice)) + "\n");
                Message.Append("MAX: $" + (float)info.MaxPrice + " ");
                Message.Append("MIN: $" + (float)info.MinPrice + "\n");
                Message.Append("RANGE: $" + (float)info.ChangeRange + "\n");
                Message.Append("NOW: [[ $" + (float)info.CurrentPrice + " ]]");
                //
                if (Math.Abs(info.CurrentPrice - lastBroudCastPrice) < ChangeRange) return;
                //
                lastBroudCastPrice = info.CurrentPrice;
            }
            //
            foreach (var userID in UsersID)
            {
                telegram.SendTextMessageAsync(new ChatId(userID), Message.ToString());
            }
        }
    }
}