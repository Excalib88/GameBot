using System;
using System.Collections.Generic;
using System.ComponentModel;
using Telegram.Bot;

namespace BotGame
{
    partial class Program
    {
        static BackgroundWorker BW;
        static ConfigSQL config;
        static TelegramBotClient Bot;

        static void Main(string[] args)
        {
            config = new ConfigSQL();

            messageOUT = new List<MessageOUT>();
            messageIN = new List<MessageIN>();

            BW = new BackgroundWorker();
            BW.DoWork += BWBot;
            
            string key = config.TOKEN;
            if (!String.IsNullOrEmpty(key) && !BW.IsBusy)
            {
                BW.RunWorkerAsync(key);
            }
            Logger.Info("start bot");

            while (true)
            {
                Console.ReadLine();
            }
        }                
    }
}