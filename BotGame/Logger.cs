using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BotGame
{
    public static class Logger
    {
        #region write
        public static void Info(string text)
        {
            WriteToLog(text, ConsoleColor.White, "Info");
        }

        public static void Success(string text)
        {
            WriteToLog(text, ConsoleColor.Green, "Success");
        }

        public static void Error(string text)
        {
            StackTrace stackTrace = new StackTrace();
            WriteToLog(text, ConsoleColor.Red, "Error][" + stackTrace.GetFrame(1).GetMethod().Name);
        }

        public static void Warn(string text)
        {
            WriteToLog(text, ConsoleColor.Gray, "Warn");
        }

        public static void Debug(string text)
        {
            WriteToLog(text, ConsoleColor.White, "Debug");
        }
        #endregion write

        private static void WriteToLog(string textLog, ConsoleColor color, string methodName)
        {
            Console.ForegroundColor = color; // устанавливаем цвет
            Console.WriteLine("[" + methodName + "] " + textLog);
            Console.ResetColor();
        }
    }
}
