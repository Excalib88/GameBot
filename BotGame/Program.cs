using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BotGame
{
    class Program
    {
        static BackgroundWorker BW;
        static ConfigSQL config;
        // static MessageIN msg;

        static void Main(string[] args)
        {
            config = new ConfigSQL();
            // msg = new MessageIN();
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

        static int getActivity = -1;
        static int game = 0;
        static bool GetActivity()
        {
            // проверяем активность
            if (getActivity == -1)
            {
                getActivity = 0;
                return true;
            }
            else
            {                
                return false;
            }
        }
        
        static bool GAME()
        {
            // проверяем на игру
            if (game == -1)
            {
                // game = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        async static void BWBot(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {                
                var Bot = new Telegram.Bot.TelegramBotClient(key);
                await Bot.SetWebhookAsync("");

                List<int> questionNumber = new List<int>();
                List<MessageOUT> messageOUT = new List<MessageOUT>();
                bool end = false;
                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {
                    // bool resultReplay = false;
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null)
                        return;

                    var update = evu.Update;
                    var message = update.Message;

                    if (message == null)
                        return;

                    int n = -1;
                    if (GetActivity())
                    {
                        var countTemp = new int[] { 7, 10, 15 };
                        Random rndTemp = new Random();
                        n = countTemp[rndTemp.Next(countTemp.Length)];

                        // получаем список вопросов
                        config.SelectQuestion();
                        var KeyId = config.issues.Keys;
                        //Random rnd = new Random();
                        var vals = KeyId.Cast<int>().ToArray();
                        //int n = 10; // количество вопросов                        
                        for (int i = 0; i < n; i++)
                        {
                            var val = vals[rndTemp.Next(vals.Length)];
                            if (!questionNumber.Contains(val))
                                questionNumber.Add(val);
                        }
                        if (questionNumber.Count < n)
                        {
                            n = questionNumber.Count;
                        }
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Внимание, начинается игра!\nВсего вопросов: " + n.ToString());
                        game = -1;
                        System.Threading.Thread.Sleep(3000);
                    }

                    if (GAME())
                    {    
                        // получаем вопрос
                        IssuesClass issues = (IssuesClass)config.issues[questionNumber[0]];

                        // если ответ через реплай
                        if (issues.TypeAnswer == 0)
                        {
                            var msgOUT = await Bot.SendTextMessageAsync(message.Chat.Id, issues.QuestionText);
                            messageOUT.Add(
                                new MessageOUT
                                {
                                    ChatId = msgOUT.Chat.Id.ToString(),
                                    MessageId = msgOUT.MessageId.ToString(),
                                    MessageText = msgOUT.Text.ToString(),
                                    MmessageDate = msgOUT.Date.ToString()
                                });
                        }
                        questionNumber.Remove(questionNumber[0]); // удаляем первый вопрос из списка вопросов на игру
                        if (questionNumber.Count == 0)
                        {
                            game = 0;
                            end = true;
                            return;
                        }
                    }                    

                    if (questionNumber.Count < 1 && game == 0 && end)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Внимание, игра закончена!");
                        config.issues = null; // очищаем список вопросов в конце игры
                        end = false;
                    }

                    //if (message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage)
                    //{
                    //    msg.ChatId = update.Message.Chat.Id.ToString();
                    //    msg.MessageId = update.Message.MessageId.ToString();
                    //    msg.UserId = message.From.Id.ToString();
                    //    msg.UserFirstName = message.From.FirstName;
                    //    msg.UserLastName = message.From.LastName;
                    //    msg.UserUsername = message.From.Username;
                    //    msg.MessageText = message.Text;
                    //    msg.MmessageDate = message.Date.ToString();

                    //    if (message.ReplyToMessage != null)
                    //    {
                    //        if (message.ReplyToMessage.From.Id.ToString() == config.IDBOT)
                    //        {
                    //            msg.ReplayToMessageId = message.ReplyToMessage.MessageId.ToString();
                    //            msg.ReplayToMessageText = message.ReplyToMessage.Text;
                    //            // msg.ReplayToUserId = message.ReplyToMessage.From.Id.ToString();
                    //            // resultReplay = true;
                    //        }
                    //    }
                    //}

                    //var KeyId = config.issues.Keys;
                    //Random rnd = new Random();                    
                    //var vals = KeyId.Cast<int>().ToArray();
                    //var val = vals[rnd.Next(vals.Length)];
                    //IssuesClass issues = (IssuesClass)config.issues[val];                    
                    //if (issues.TypeAnswer == 1)
                    //{
                    //    await Bot.SendTextMessageAsync(msg.ChatId, issues.QuestionText);
                    //}                    
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