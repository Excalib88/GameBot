using System;
using System.Diagnostics;

namespace BotGame
{
    public static class Logger
    {
        public const string TIME_FORMAT = @"yyyy-MM-dd HH:mm:ss";

        #region write
        public static void Info(string text)
        {
            WriteToLog(text, ConsoleColor.White, "info");
        }

        public static void Success(string text)
        {
            WriteToLog(text, ConsoleColor.DarkGreen, "success");
        }

        public static void Error(string text)
        {
            StackTrace stackTrace = new StackTrace();
            WriteToLog(text, ConsoleColor.DarkRed, "error][" + stackTrace.GetFrame(1).GetMethod().Name);
        }

        public static void Warn(string text)
        {
            WriteToLog(text, ConsoleColor.DarkGray, "warn");
        }

        public static void Debug(string text)
        {
            WriteToLog(text, ConsoleColor.Cyan, "debug");
        }

        public static void DebugMessage(Telegram.Bot.Types.Message msg)
        {
            try
            {
                WriteToLog(msg.Caption, ConsoleColor.Cyan, "debug");
            }
            catch { }
            try
            { 
            WriteToLog(msg.Chat.Description, ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.FirstName, ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.Id.ToString(), ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.InviteLink.ToString(), ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.LastName.ToString(), ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.Title.ToString(), ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.Type.ToString(), ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Chat.Username.ToString(), ConsoleColor.Cyan, "Chat");
            }
            catch { }
            try
            {
                WriteToLog(msg.Text.ToString(), ConsoleColor.Cyan, "Text");
            }
            catch { }
            try
            {
                WriteToLog(msg.Type.ToString(), ConsoleColor.Cyan, "Type");
            }
            catch { }
            try
            {
                WriteToLog(msg.From.FirstName.ToString(), ConsoleColor.Cyan, "From");
            }
            catch { }
            try
            {
                WriteToLog(msg.From.Id.ToString(), ConsoleColor.Cyan, "From");
            }
            catch { }
            try
            {
                WriteToLog(msg.From.LanguageCode.ToString(), ConsoleColor.Cyan, "From");
            }
            catch { }
            try
            {
                WriteToLog(msg.From.LastName.ToString(), ConsoleColor.Cyan, "From");
            }
            catch { }
            try
            {
                WriteToLog(msg.From.Username.ToString(), ConsoleColor.Cyan, "From");
            }
            catch { }
            try
            {
                WriteToLog(msg.ReplyToMessage.Text.ToString(), ConsoleColor.Cyan, "From");
            }
            catch { }
        }

        #endregion write

        private static void WriteToLog(string textLog, ConsoleColor color, string methodName)
        {
            DateTime dtnow = DateTime.Now;
            Console.ForegroundColor = color;
            Console.WriteLine(dtnow.ToString(TIME_FORMAT) + " [" + methodName + "] " + textLog);
            Console.ResetColor();
        }
    }
}