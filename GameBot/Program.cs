using System;
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

            BW = new BackgroundWorker();
            BW.DoWork += BWBot;
            
            string key = config.TOKEN;
            if (!String.IsNullOrEmpty(key) && !BW.IsBusy)
            {
                BW.RunWorkerAsync(key);                
                Logger.Info("start bot");
            }            

            while (true)
            {
                //Console.ReadLine();
            }
        }                
    }
}