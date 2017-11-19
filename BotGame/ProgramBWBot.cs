using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotGame
{
    partial class Program
    {
        static IssuesClass newQuestion;
        static MessageOUT msgOUTobject = null;
        static MessageIN msgINobject = null;
        static IssuesClass issuesObject = null;
        static List<MessageOUT> messageOUTobject = new List<MessageOUT>();
        static List<MessageIN> messageINobject = new List<MessageIN>();
        static List<int> questionNumber;
        static int num = 0;
        static bool end = false;
        static bool game = false;

        static bool insert = false;

        static bool StartGame(string text, int idUser)
        {            
            if ((text == @"/newgame" || text == @"/newgame@ucs13bot") && !game && idUser == config.ADMIN)
            {
                game = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        static async Task<MessageOUT> SendIssuesReply(
            TelegramBotClient Bot, long chatId, string num, IssuesClass issues, string textStart = "", int replayMsgId = 0)
        {
            string txtQuest = issues.QuestionText;
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? "\n\nВарианты ответов:\n1 - " + 
                issues.PossibleAnswer_1 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? "\n2 - " + issues.PossibleAnswer_2 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? "\n3 - " + issues.PossibleAnswer_3 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_4) ? "\n4 - " + issues.PossibleAnswer_4 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_5) ? "\n5 - " + issues.PossibleAnswer_5 : "";

            Telegram.Bot.Types.Message msg;
            if (replayMsgId == 0)
                msg = await Bot.SendTextMessageAsync(chatId, textStart + "Вопрос " + num + "\n" + txtQuest);
            else
                msg = await Bot.SendTextMessageAsync(chatId, textStart + "Вопрос " + num + "\n" + txtQuest,
            replyToMessageId: msgINobject.MessageId);

            if (!String.IsNullOrEmpty(textStart))
                msg.Text = "Вопрос " + num + "\n" + txtQuest;

            MessageOUT msgOUTtemp = await SaveMsgOUT(msg, issues.Id);
            Logger.Info(num.ToString() + " question submitted");
            return msgOUTtemp;
        }

        static async Task<MessageOUT> SendIssuesButton(
            TelegramBotClient Bot, long chatId, string num, IssuesClass issues, string textStart = "", int replayMsgId = 0)
        {
            string btn1 = !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? issues.PossibleAnswer_1 : "";
            string btn2 = !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? issues.PossibleAnswer_2 : "";
            string btn3 = !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? issues.PossibleAnswer_3 : "";

            var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                new InlineKeyboardCallbackButton(btn1, "1"),
                new InlineKeyboardCallbackButton(btn2, "2"),
                new InlineKeyboardCallbackButton(btn3, "3"),
            });

            Telegram.Bot.Types.Message msg;
            if (replayMsgId == 0)
                msg = await Bot.SendTextMessageAsync(chatId, textStart + "Вопрос " + num + "\n" + issues.QuestionText,
                    replyMarkup: replyMarkup);
            else
                msg = await Bot.SendTextMessageAsync(chatId, textStart + "Вопрос " + num + "\n" + issues.QuestionText,
                    replyMarkup: replyMarkup, replyToMessageId: msgINobject.MessageId);

            MessageOUT msgOUTtemp = await SaveMsgOUT(msg, issues.Id);
            Logger.Info(num.ToString() + " question submitted");
            return msgOUTtemp;
        }

        static async Task<MessageOUT> SaveMsgOUT(Telegram.Bot.Types.Message message, int issuesId)
        {
            MessageOUT msgOUTtemp = new MessageOUT
            {
                QuestionId = issuesId,
                ChatId = message.Chat.Id,
                MessageId = message.MessageId,
                MessageText = message.Text,
                MmessageDate = message.Date,
                userWin = new User()
        };
            messageOUTobject.Add(msgOUTtemp);
            return msgOUTtemp;
        }

        static string GetUserName(Telegram.Bot.Types.Message message)
        {
            string userName = String.IsNullOrEmpty(message.From.LastName) ? message.From.FirstName : message.From.FirstName
                        + " " + message.From.LastName;
            return userName;
        }

        static async Task<MessageIN> SaveMsgIn(Telegram.Bot.Types.Message message)
        {
            MessageIN msgINtemp = new MessageIN
            {
                ChatId = message.Chat.Id,
                MessageId = message.MessageId,
                MessageText = message.Text,                
                ReplayToMessageId = message.ReplyToMessage == null ? -1 : message.ReplyToMessage.MessageId,
                ReplayToMessageText = message.ReplyToMessage == null ? "" : message.ReplyToMessage.Text,
                ReplayToUserId = message.ReplyToMessage == null ? -1 : message.ReplyToMessage.From.Id,
                userAttempt = new User { Id = message.From.Id, Name = GetUserName(message), Username = message.From.Username }
            };
            
            messageINobject.Add(msgINtemp);
            return msgINtemp;
        }

        async static void BWBot(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var key = e.Argument as String;
            try
            {
                Bot = new TelegramBotClient(key);
                await Bot.SetWebhookAsync("");

                questionNumber = new List<int>();                                                                     

                Bot.OnUpdate += async (object su, UpdateEventArgs evu) =>
                {                    
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null)
                        return;
                                        
                    var message = evu.Update.Message;

                    if (message == null || message.Type != MessageType.TextMessage)
                        return;

                    if (((message.Text.StartsWith("/insert") || message.Text.StartsWith("/insert@ucs13bot"))
                        && (message.Chat.Type == ChatType.Private)) || insert)
                    {
                        // добавление вопросов  
                        await Insert(message);
                    }

                    string textStart = "";
                    if (StartGame(message.Text, message.From.Id))
                    {
                        var m = await SaveMsgIn(message);

                        Logger.Success("start game");
                        var countTemp = new int[] { 7, 10, 15 };
                        Random rndTemp = new Random();
                        int n = countTemp[rndTemp.Next(countTemp.Length)];

                        // получаем список вопросов
                        config.SelectQuestion();
                        var KeyId = config.issues.Keys;                        
                        var vals = KeyId.Cast<int>().ToArray();                        
                        for (int i = 0; i < n; i++)
                        {
                            var val = vals[rndTemp.Next(vals.Length)];
                            if (!questionNumber.Contains(val))
                                questionNumber.Add(val);
                        }
                        Logger.Info("Всего вопросов: " + questionNumber.Count.ToString());
                        textStart += "Игра началась!\nВсего вопросов: " + questionNumber.Count.ToString() + "\n\n";
                        game = true;                                                
                    }

                    if (game)
                    {
                        // получаем вопрос
                        issuesObject = (IssuesClass)config.issues[questionNumber[0]];

                        // если ответ через реплай
                        if (issuesObject.TypeAnswer == 0)
                        {
                            if (msgOUTobject is null)
                            {
                                num++;                                
                                msgOUTobject = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), issuesObject, textStart);
                                textStart = "";
                            }
                            else
                            {
                                if (message.ReplyToMessage != null)
                                {
                                    msgINobject = await SaveMsgIn(message);

                                    if (msgINobject.ReplayToUserId == config.IDBOT && msgOUTobject.MessageId == msgINobject.ReplayToMessageId)
                                    {
                                        msgOUTobject.AttemptsAnswers++;
                                        if (issuesObject.CorrectAnswer.ToLower().Trim() == msgINobject.MessageText.ToLower().Trim())
                                        {
                                            Logger.Success(msgINobject.userAttempt.Name + " correct unswer");
                                            msgOUTobject.AnswerDate = msgINobject.MmessageDate;
                                            msgOUTobject.userWin = msgINobject.userAttempt;
                                            await Answer(message);
                                        }
                                        else
                                        {
                                            // считаем ответ как попытку ответить
                                            Logger.Info(msgINobject.userAttempt.Name + " incorrect unswer");
                                            msgINobject = null;
                                        }                                        
                                    }
                                }                             
                            }
                        }// если ответ через реплай
                        else
                        // если ответ через кнопки
                        if ((issuesObject.TypeAnswer == 1) && (msgOUTobject is null))
                        {
                            num++;
                            msgOUTobject = await SendIssuesButton(Bot, message.Chat.Id, num.ToString(), issuesObject);                                                   
                        }// если ответ через кнопки                       
                    }

                    if (questionNumber.Count < 1 && !game && end)
                    {
                        end = false;
                        EndGame(message);
                    }
                };
                
                Bot.OnCallbackQuery += async (object sc, CallbackQueryEventArgs ev) =>
                {
                    await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    var message = ev.CallbackQuery.Message;
                    message.From = ev.CallbackQuery.From;
                    msgINobject = await SaveMsgIn(message);
                    msgOUTobject.AttemptsAnswers++;
                    if (issuesObject.CorrectAnswer.ToLower().Trim() == ev.CallbackQuery.Data.ToLower().Trim())
                    {                        
                        await Bot.EditMessageTextAsync(msgOUTobject.ChatId, msgOUTobject.MessageId, msgOUTobject.MessageText, 
                            parseMode: ParseMode.Default, replyMarkup: null);
                        
                        msgOUTobject.userWin = msgINobject.userAttempt;

                        string name = String.IsNullOrEmpty(ev.CallbackQuery.From.LastName) ? ev.CallbackQuery.From.FirstName : ev.CallbackQuery.From.FirstName
                        + " " + ev.CallbackQuery.From.LastName;

                        var msgTemp = await Bot.SendTextMessageAsync(msgOUTobject.ChatId, "Правильный ответ '" + 
                            issuesObject.CorrectAnswer + "' получен от " + name);
                        
                        msgOUTobject = await SaveMsgOUT(msgTemp, issuesObject.Id);
                        msgOUTobject = null;

                        await Answer(message);
                    }
                    else
                    {
                        Logger.Info(msgINobject.userAttempt.Name + " incorrect unswer");
                        //msgOUT.AttemptsAnswers++;
                        msgINobject = null;
                    }

                    if (questionNumber.Count < 1 && !game && end)
                    {
                        end = false;
                        EndGame(message);
                    }
                };

                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Logger.Error(ex.Message);
            }            
        }

        static async Task Answer(Telegram.Bot.Types.Message message, string button = "")
        {           
            questionNumber.Remove(questionNumber[0]);
            num++;
            if (questionNumber.Count != 0)
            {
                string temp = "Правильный ответ\n\n";
                issuesObject = (IssuesClass)config.issues[questionNumber[0]];
                if (questionNumber.Count > 1)
                    temp += "Следующий вопрос:\n\n";
                
                if (issuesObject.TypeAnswer == 0)
                    msgOUTobject = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), issuesObject, temp, message.MessageId);
                if (issuesObject.TypeAnswer == 1)
                    msgOUTobject = await SendIssuesButton(Bot, message.Chat.Id, num.ToString(), issuesObject, temp, message.MessageId);
            }
            else
            {
                game = false;
                end = true;
            }
            msgINobject = null;
        }

        static async void EndGame(Telegram.Bot.Types.Message message)
        {
            //var msgTemp = await Bot.SendTextMessageAsync(message.Chat.Id, "Конец игры");
            //MessageOUT msgOUT = await SaveMsgOUT(msgTemp, 0);            
            //msgOUT = null;
            Logger.Success("end game");            
            string win = Statistics.GetStatistics(messageOUTobject, config);
            config.issues.Clear();
            await Bot.SendTextMessageAsync(message.Chat.Id, win);
            await Task.Delay(config.DeletionDelay);
            DeleteMsg();
            num = 0;
        }

        static async void DeleteMsg()
        {
            Logger.Info("start delete message");
            foreach (MessageOUT msgDel in messageOUTobject)
            {
                if (msgDel != null)
                    try
                    {
                        await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                        Logger.Warn(msgDel.MessageText);
                    }
            }

            foreach (MessageIN msgDel in messageINobject)
            {
                if (msgDel != null)
                    try
                    {
                        await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error(e.Message);
                        Logger.Warn(msgDel.MessageText);
                    }
            }
            messageOUTobject.Clear();
            messageINobject.Clear();
            Logger.Info("end delete message");
        }

        static async public void SendMsg(long chat_id, string text)
        {
            var msgTemp = await Bot.SendTextMessageAsync(chat_id, text);
            MessageOUT msgOUT = await SaveMsgOUT(msgTemp, 0);
            msgOUT = null;
        }

        static int flag = -1;
        static int count = -1;
        static int countQ = 0;
        static async public Task Insert(Telegram.Bot.Types.Message message)
        {
            //await Bot.SendTextMessageAsync(message.Chat.Id, "Введите текст вопроса");
            insert = true;
            if (newQuestion == null)
                newQuestion = new IssuesClass();

            if (String.IsNullOrEmpty(newQuestion.QuestionText) && flag == -1)
            {
                await SendInsertMsg(message.Chat.Id, "Введите текст вопроса", 0);
                return;
            }
            if (flag == 0)
            {
                if (!String.IsNullOrEmpty(message.Text))
                {
                    newQuestion.QuestionText = message.Text.Replace("\n", "@BR");
                    flag = 1;
                }               
            }
            if (flag == 1)
            {
                await SendInsertMsg(message.Chat.Id, "Введите количество вариантов ответа (максимум 5)", 2);
                return;
            }
            if (flag == 2)
            {
                if (int.TryParse(message.Text, out count))
                {
                    //count = Convert.ToInt32(message.Text);
                    flag = 3;
                    //return;
                }
                else
                {
                    await SendInsertMsg(message.Chat.Id, "Введите количество вариантов ответа (максимум 5)", 2);
                    return;
                }
            }

            if (flag == 3)
            {
                if (countQ <= count)
                {                    
                    switch (countQ)
                    {
                        case 1:
                            {
                                newQuestion.PossibleAnswer_1 = message.Text;
                                break;
                            }
                        case 2:
                            {
                                newQuestion.PossibleAnswer_2 = message.Text;
                                break;
                            }
                        case 3:
                            {
                                newQuestion.PossibleAnswer_3 = message.Text;
                                break;
                            }
                        case 4:
                            {
                                newQuestion.PossibleAnswer_4 = message.Text;
                                break;
                            }
                        case 5:
                            {
                                newQuestion.PossibleAnswer_5 = message.Text;
                                break;
                            }
                    }
                    if (countQ < count)
                    {
                        await SendInsertMsg(message.Chat.Id, "Введите " + (countQ + 1).ToString() + " вариант ответа", 3);
                        countQ++;
                        return;
                    }
                    else
                    {
                        await SendInsertMsg(message.Chat.Id, "Введите номер верного варианта ответа", 4);
                        return;
                    }
                }                
            }

            if (flag == 4)
            {
                if (int.TryParse(message.Text, out int n))
                {
                    //count = Convert.ToInt32(message.Text);
                    newQuestion.CorrectAnswer = n.ToString();
                    flag = 5;
                }
                else
                {
                    await SendInsertMsg(message.Chat.Id, "Введите номер верного варианта ответа", 4);
                    return;
                }
            }

            if (flag == 5)
            {
                await SendInsertMsg(message.Chat.Id, "Введите 0, если ответ через реплай\n1, если ответ через кнопки", 6);
                return;
            }

            if (flag == 6)
            {
                if (int.TryParse(message.Text, out int n))
                {
                    //count = Convert.ToInt32(message.Text);
                    newQuestion.TypeAnswer = n;
                    flag = 7;
                }
                else
                {
                    await SendInsertMsg(message.Chat.Id, "Введите 0, если ответ через реплай или 1, если ответ через кнопки", 6);
                    return;
                }
            }

            if (flag == 7)
            {
                string txtQuest = "Проверьте правильнность введенных данных:\n\n" + newQuestion.QuestionText;
                txtQuest += !String.IsNullOrEmpty(newQuestion.PossibleAnswer_1) ? "\n\nВарианты ответов:\n1 - " +
                    newQuestion.PossibleAnswer_1 : "";
                txtQuest += !String.IsNullOrEmpty(newQuestion.PossibleAnswer_2) ? "\n2 - " + newQuestion.PossibleAnswer_2 : "";
                txtQuest += !String.IsNullOrEmpty(newQuestion.PossibleAnswer_3) ? "\n3 - " + newQuestion.PossibleAnswer_3 : "";
                txtQuest += !String.IsNullOrEmpty(newQuestion.PossibleAnswer_4) ? "\n4 - " + newQuestion.PossibleAnswer_4 : "";
                txtQuest += !String.IsNullOrEmpty(newQuestion.PossibleAnswer_5) ? "\n5 - " + newQuestion.PossibleAnswer_5 : "";
                txtQuest += "\n\nВерный ответ - " + newQuestion.CorrectAnswer;
                txtQuest += "\n\nОтвет с помощью ";
                txtQuest += newQuestion.TypeAnswer == 0 ? "реплая" : "кнопок";
                txtQuest += "\n\nЕсли все верно - введите 1, иначе 0";

                await SendInsertMsg(message.Chat.Id, txtQuest, 8);
                return;
            }

            if (flag == 8)
            {
                // insert in base
                insert = false;
            }
        }

        static async Task SendInsertMsg(long chatId, string text, int i)
        {
            await Bot.SendTextMessageAsync(chatId, text);
            flag = i;
        }
    }
}