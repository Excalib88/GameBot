using System;
using System.Diagnostics;

namespace BotGame
{
    public static class Logger
    {
        public const string TIME_FORMAT = @"yyyy-MM-dd HH:mm:ss";
        public const string DATE_FORMAT = @"yyyy-MM-dd";

        #region write

        public static void Info(string text)
        {
            WriteToLog(text, ConsoleColor.DarkBlue, "info");
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
            WriteToLog(text, ConsoleColor.White, "warn");
        }

        public static void Debug(string text)
        {
            WriteToLog(text, ConsoleColor.Cyan, "debug");
        }

        // todo:debugMsg

        #endregion write

        private static void WriteToLog(string textLog, ConsoleColor color, string methodName)
        {
            DateTime currentDateTime = DateTime.Now;
            string currentDateTimeString = currentDate.ToString(TIME_FORMAT);
            string logMessage = String.Format("{0} [{1}] {2}", currentDateString, methodName, textLog);

            Console.ForegroundColor = color;

            Console.WriteLine(logMessage);
            Logger.WriteLogToFile(logMessage, currentDateTime);

            Console.ResetColor();
        }
        private async static void WriteLogToFile(string logText, DateTime currentDateTime)
        {
            string currentDateString = currentDateTime.ToString(DATE_FORMAT);
            string logFileDirectory = String.Format(@"log\log{0}.txt", currentDateString);
            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
            byte[] logData = unicodeEncoding.GetBytes(String.Format("{0}:{1}{2}", currentDateTime.ToString(TIME_FORMAT), logText, Environment.NewLine));

            try
            {
                using (FileStream logStream = new FileStream(logFileDirectory, FileMode.OpenOrCreate))
                {
                    logStream.Seek(0, SeekOrigin.End);
                    await logStream.WriteAsync(logData, 0, logData.Length);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }
    }
}