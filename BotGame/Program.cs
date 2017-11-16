using System;
using System.ComponentModel;
using System.Linq;

namespace BotGame
{
    class Program
    {
        static BackgroundWorker BW;
        static ConfigSQL config;
        static Message msg;

        static void Main(string[] args)
        {
            config = new ConfigSQL();
            msg = new Message();
            BW = new BackgroundWorker();
            BW.DoWork += BWBot;
            
            string key = config.KEY;
            if (!String.IsNullOrEmpty(key) && !BW.IsBusy)
            {
                BW.RunWorkerAsync(key);
            }
            Console.WriteLine("start bot");
            Console.ReadLine();
        }

        async static void BWBot(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {                
                var Bot = new Telegram.Bot.TelegramBotClient(key);
                await Bot.SetWebhookAsync("");               

                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null)
                        return;

                    var update = evu.Update;
                    var message = update.Message;

                    if (message == null)
                        return;

                    if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                    {
                        msg.ChatId = update.Message.Chat.Id;
                        msg.MessageId = update.Message.MessageId;
                        msg.UserId = message.From.Id;
                        msg.UserFirstName = message.From.FirstName;
                        msg.UserLastName = message.From.LastName;
                        msg.UserUsername = message.From.Username;
                        msg.MessageText = message.Text;
                        msg.MmessageDate = message.Date.ToString();
                                                
                        //msg.ReplayToMessageId = message.ReplyToMessage.MessageId;
                        //msg.ReplayToMessageText = message.ReplyToMessage.Text;
                        //msg.ReplayToUserId = message.ReplyToMessage.From.Id;
                    }

                    var KeyId = config.issues.Keys;
                    Random rnd = new Random();                    
                    var vals = KeyId.Cast<int>().ToArray();
                    var val = vals[rnd.Next(vals.Length)];

                    IssuesClass issues = (IssuesClass)config.issues[val];

                    await Bot.SendTextMessageAsync(msg.ChatId, issues.QuestionText);
                };                
                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}