using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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
        public static void broadcastMessage(string MSG)
        {
            foreach (var userID in UsersID)
            {
                telegram.SendTextMessageAsync(new ChatId(userID), MSG);
            }
        }
    }
}