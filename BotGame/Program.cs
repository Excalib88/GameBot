using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

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

            while (true)
            {
                Console.ReadLine();
            }
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

        static async Task<MessageOUT> SendIssues(TelegramBotClient Bot, long chatId, string num, string questionText)
        {
            var msg = await Bot.SendTextMessageAsync(chatId, "Вопрос " + num + "\n" + questionText);
            MessageOUT msgOUT = new MessageOUT
            {
                ChatId = msg.Chat.Id.ToString(),
                MessageId = msg.MessageId.ToString(),
                MessageText = msg.Text.ToString(),
                MmessageDate = msg.Date.ToString()
            };

            //return Task.Run(() =>
            //{
                return msgOUT;
           // });
        }

        async static void BWBot(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {                
                var Bot = new TelegramBotClient(key);
                await Bot.SetWebhookAsync("");

                List<int> questionNumber = new List<int>();
                MessageOUT msgOUT = null;
                MessageIN msgIN = null;
                List<MessageOUT> messageOUT = new List<MessageOUT>();
                List<MessageIN> messageIN = new List<MessageIN>();
                int num = 0;
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
                        // получаем вопро
                        IssuesClass issues = (IssuesClass)config.issues[questionNumber[0]];

                        // если ответ через реплай
                        if (issues.TypeAnswer == 0)
                        {
                            if (msgOUT is null)
                            {
                                num++;
                                msgOUT = await SendIssues(Bot, message.Chat.Id, num.ToString(), issues.QuestionText);                           
                                messageOUT.Add(msgOUT);
                            }
                            // проверка ответа
                            if (msgOUT != null)
                                if (message.ReplyToMessage != null)
                                {
                                    if (message.ReplyToMessage.From.Id.ToString() == config.IDBOT)
                                    {
                                        msgIN = new MessageIN
                                        {
                                            MessageId = message.MessageId.ToString(),
                                            MessageText = message.Text,
                                            UserFirstName = message.From.FirstName,
                                            ReplayToMessageId = message.ReplyToMessage.MessageId.ToString(),
                                            ReplayToMessageText = message.ReplyToMessage.Text
                                        };
                                        messageIN.Add(msgIN);
                                    }

                                    if (msgOUT.MessageId == msgIN.ReplayToMessageId)
                                    {
                                        if (issues.CorrectAnswer.ToLower().Trim() == msgIN.MessageText.ToLower().Trim())
                                        {
                                            await Bot.SendTextMessageAsync(message.Chat.Id, "Правильный ответ!", replyToMessageId: Convert.ToInt32(msgIN.MessageId));
                                            msgOUT = null;
                                            questionNumber.Remove(questionNumber[0]);
                                            num++;
                                            if (questionNumber.Count != 0)
                                            {
                                                issues = (IssuesClass)config.issues[questionNumber[0]];
                                                msgOUT = await SendIssues(Bot, message.Chat.Id, num.ToString(), issues.QuestionText);
                                                messageOUT.Add(msgOUT);
                                            }
                                            else
                                            {
                                                //msgOUT = null;
                                            }
                                        }
                                    }
                                }
                            // проверка ответа
                        }
                        //if (msgOUT is null)
                        {
                            // questionNumber.Remove(questionNumber[0]); // удаляем первый вопрос из списка вопросов на игру

                            if (questionNumber.Count == 0)
                            {
                                game = 0;
                                end = true;
                            }
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