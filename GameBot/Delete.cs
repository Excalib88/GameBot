using System;
using System.Collections;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace BotGame
{
    public static class Delete
    {
        public static async Task DeleteQuestion(Telegram.Bot.Types.Message message, Hashtable deleteUser,
            ConfigSQL config, TelegramBotClient Bot)
        {
            DeleteOptions deleteOptions = new DeleteOptions();
            if (!deleteUser.ContainsKey(message.From.Id))
            {
                deleteOptions.delete = true;
                deleteOptions = config.SelectForDelete();
                SendMsgDelete(Bot, deleteOptions, message);
                deleteUser.Add(message.From.Id, deleteOptions);
                return;
            }
            if (message.Text.StartsWith("/delete_"))
            {
                int s = Convert.ToInt32(message.Text.Replace("/delete_", ""));
                bool b = config.DeleteIssue(s);
                if (b)
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Вопрос удален", parseMode: ParseMode.Default);
                    Logger.Info(message.From.FirstName + " insert true");
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ошибка при удалении вопроса", parseMode: ParseMode.Default);
                    Logger.Info(message.From.FirstName + " insert false");
                }
                deleteOptions.issuesDelete.Remove(s);
            }
            if (message.Text == "/next10")
            {
                deleteOptions = (DeleteOptions)deleteUser[message.From.Id];
                SendMsgDelete(Bot, deleteOptions, message);
            }
        }

        static async void SendMsgDelete(TelegramBotClient Bot, 
            DeleteOptions deleteOptions, Telegram.Bot.Types.Message message)
        {
            string textMsg = "";
            int n = 0;
            for (int i = deleteOptions.begin; i < deleteOptions.maxId; i++)
            {
                string s = (String)deleteOptions.issuesDelete[i];
                if (!String.IsNullOrEmpty(s))
                {
                    textMsg += i.ToString() + ". " + s + " /delete_" + i.ToString() + "\n\n";
                    n++;
                }
                if (i == deleteOptions.maxId - 1)
                {
                    deleteOptions.begin = 0;
                    textMsg += "/next10";
                    break;
                }
                if (n == deleteOptions.count)
                {
                    deleteOptions.begin = i + 1;
                    textMsg += "/next10";
                    break;
                }
            }
            try
            {
                deleteOptions.msg = await Bot.SendTextMessageAsync(message.Chat.Id, textMsg, parseMode: ParseMode.Html);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException e)
            {
                Logger.Warn(e.Message);
                deleteOptions.msg = await Bot.SendTextMessageAsync(message.Chat.Id, textMsg, parseMode: ParseMode.Default);
            }
        }
    }
}