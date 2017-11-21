using System;
using System.Collections;
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
        private static Hashtable gameChat = new Hashtable();

        static bool insert = false;
        private static Hashtable insertUser = new Hashtable();

        static bool StartGame(Telegram.Bot.Types.Message message)
        {
            if (!gameChat.ContainsKey(message.Chat.Id)                
                && message.From.Id == config.ADMIN)
            {
                gameChat.Add(message.Chat.Id,
                    new Game
                    {
                        msgOUTobject = null,
                        msgINobject = null,
                        issuesObject = null,
                        messageOUTobject = new List<MessageOUT>(),
                        messageINobject = new List<MessageIN>(),
                        questionNumber = new List<int>(),
                        num = 0,
                        end = false,
                        game = true
                    });
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
            Game gameObject = (Game)gameChat[chatId];

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
            replyToMessageId: gameObject.msgINobject.MessageId);

            if (!String.IsNullOrEmpty(textStart))
                msg.Text = "Вопрос " + num + "\n" + txtQuest;

            MessageOUT msgOUTtemp = await SaveMsgOUT(msg, issues.Id);
            Logger.Info("chat " + chatId.ToString() + " " + num.ToString() + " question submitted");
            return msgOUTtemp;
        }

        static async Task<MessageOUT> SendIssuesButton(
            TelegramBotClient Bot, long chatId, string num, IssuesClass issues, string textStart = "", int replayMsgId = 0)
        {
            Game gameObject = (Game)gameChat[chatId];

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
                    replyMarkup: replyMarkup, replyToMessageId: gameObject.msgINobject.MessageId);

            MessageOUT msgOUTtemp = await SaveMsgOUT(msg, issues.Id);
            Logger.Info("chat " + chatId.ToString() + " " + num.ToString() + " question submitted");
            return msgOUTtemp;
        }

        static async Task<MessageOUT> SaveMsgOUT(Telegram.Bot.Types.Message message, int issuesId)
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];

            MessageOUT msgOUTtemp = new MessageOUT
            {
                QuestionId = issuesId,
                ChatId = message.Chat.Id,
                MessageId = message.MessageId,
                MessageText = message.Text,
                MmessageDate = message.Date,
                userWin = new User()
        };
            gameObject.messageOUTobject.Add(msgOUTtemp);
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
            Game gameObject = (Game)gameChat[message.Chat.Id];

            MessageIN msgINtemp = new MessageIN
            {
                ChatId = message.Chat.Id,
                MessageId = message.MessageId,
                MessageText = message.Text,
                MmessageDate = message.Date,
                ReplayToMessageId = message.ReplyToMessage == null ? -1 : message.ReplyToMessage.MessageId,
                ReplayToMessageText = message.ReplyToMessage == null ? "" : message.ReplyToMessage.Text,
                ReplayToUserId = message.ReplyToMessage == null ? -1 : message.ReplyToMessage.From.Id,
                userAttempt = new User { Id = message.From.Id, Name = GetUserName(message), Username = message.From.Username }
            };

            gameObject.messageINobject.Add(msgINtemp);
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

                Bot.OnUpdate += async (object su, UpdateEventArgs evu) =>
                {                    
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null)
                        return;
                                        
                    var message = evu.Update.Message;

                    if (message == null || message.Type != MessageType.TextMessage)
                        return;

                    Game gameObject = (Game)gameChat[message.Chat.Id];

                    if ((message.Text.StartsWith("/insert") || message.Text.StartsWith("/insert@ucs13bot")) || insert)
                        if (message.Chat.Type == ChatType.Private)
                        {
                            if (!insertUser.ContainsKey(message.Chat.Id))
                            {
                                insertUser.Add(message.Chat.Id,
                                    new InsertOptions
                                    {
                                        flag = -1,
                                        count = -1,
                                        countQ = 0,
                                        newQuestion = new IssuesClass()
                                    });
                            }
                            await Insert(message);
                            return;                            
                        }

                    string textStart = "";
                    if (message.Text == @"/newgame" || message.Text == @"/newgame@ucs13bot")
                        if (StartGame(message))
                        {                            
                            var m = await SaveMsgIn(message);
                            gameObject = (Game)gameChat[message.Chat.Id];
                            Logger.Success("chat " + message.Chat.Id.ToString() + " start game");
                            var countTemp = new int[] { 7, 10, 15 };
                            Random rndTemp = new Random();
                            int n = countTemp[rndTemp.Next(countTemp.Length)];

                            // получаем список вопросов
                            gameObject.issues = config.SelectQuestion();
                            var KeyId = gameObject.issues.Keys;
                            var vals = KeyId.Cast<int>().ToArray();
                            for (int i = 0; i < n; i++)
                            {
                                var val = vals[rndTemp.Next(vals.Length)];
                                if (!gameObject.questionNumber.Contains(val))
                                    gameObject.questionNumber.Add(val);
                            }
                            Logger.Info("chat " + message.Chat.Id.ToString() + " всего вопросов: " + gameObject.questionNumber.Count.ToString());
                            string count = config.SelectCountGame(message.Chat.Id);
                            textStart += "Игра №" + count + " началась!\nВсего вопросов: " + gameObject.questionNumber.Count.ToString() + "\n\n";
                            gameObject.game = true;
                        }

                    if (gameObject.game)
                    {
                        // получаем вопрос
                        gameObject.issuesObject = (IssuesClass)gameObject.issues[gameObject.questionNumber[0]];
                        
                        // если ответ через реплай
                        if (gameObject.issuesObject.TypeAnswer == 0)
                        {
                            if (gameObject.msgOUTobject is null)
                            {
                                gameObject.num++;
                                gameObject.msgOUTobject = await SendIssuesReply(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, textStart);
                                textStart = "";
                            }
                            else
                            {
                                if (message.ReplyToMessage != null)
                                {
                                    gameObject.msgINobject = await SaveMsgIn(message);

                                    if (gameObject.msgINobject.ReplayToUserId == config.IDBOT && gameObject.msgOUTobject.MessageId == gameObject.msgINobject.ReplayToMessageId)
                                    {
                                        gameObject.msgOUTobject.AttemptsAnswers++;
                                        if (gameObject.issuesObject.CorrectAnswer.ToLower().Trim() == gameObject.msgINobject.MessageText.ToLower().Trim())
                                        {
                                            Logger.Success("chat " + message.Chat.Id.ToString() + " " + gameObject.msgINobject.userAttempt.Name + " correct unswer");
                                            gameObject.msgOUTobject.AnswerDate = gameObject.msgINobject.MmessageDate;
                                            gameObject.msgOUTobject.userWin = gameObject.msgINobject.userAttempt;
                                            await Answer(message);
                                        }
                                        else
                                        {
                                            // считаем ответ как попытку ответить
                                            Logger.Info("chat " + message.Chat.Id.ToString() + " " + gameObject.msgINobject.userAttempt.Name + " incorrect unswer");
                                            gameObject.msgINobject = null;
                                        }                                        
                                    }
                                }                             
                            }
                        }// если ответ через реплай
                        else
                        // если ответ через кнопки
                        if ((gameObject.issuesObject.TypeAnswer == 1) && (gameObject.msgOUTobject is null))
                        {
                            gameObject.num++;
                            gameObject.msgOUTobject = await SendIssuesButton(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject);                                                   
                        }// если ответ через кнопки                       
                    }

                    if (gameObject.questionNumber.Count < 1 && !gameObject.game && gameObject.end)
                    {
                        gameObject.end = false;
                        EndGame(message);
                    }
                };
                
                Bot.OnCallbackQuery += async (object sc, CallbackQueryEventArgs ev) =>
                {
                    await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    var message = ev.CallbackQuery.Message;

                    Game gameObject = (Game)gameChat[message.Chat.Id];
                                       
                    message.From = ev.CallbackQuery.From;
                    gameObject.msgINobject = await SaveMsgIn(message);
                    gameObject.msgOUTobject.AttemptsAnswers++;
                    if (gameObject.issuesObject.CorrectAnswer.ToLower().Trim() == ev.CallbackQuery.Data.ToLower().Trim())
                    {                        
                        await Bot.EditMessageTextAsync(gameObject.msgOUTobject.ChatId, gameObject.msgOUTobject.MessageId, gameObject.msgOUTobject.MessageText, 
                            parseMode: ParseMode.Default, replyMarkup: null);

                        gameObject.msgOUTobject.userWin = gameObject.msgINobject.userAttempt;
                        gameObject.msgOUTobject.AnswerDate = gameObject.msgINobject.MmessageDate;

                        string name = String.IsNullOrEmpty(ev.CallbackQuery.From.LastName) ? ev.CallbackQuery.From.FirstName : ev.CallbackQuery.From.FirstName
                        + " " + ev.CallbackQuery.From.LastName;

                        var msgTemp = await Bot.SendTextMessageAsync(gameObject.msgOUTobject.ChatId, "Правильный ответ '" +
                            gameObject.issuesObject.CorrectAnswer + "' получен от " + name);

                        gameObject.msgOUTobject = await SaveMsgOUT(msgTemp, gameObject.issuesObject.Id);
                        gameObject.msgOUTobject = null;

                        await Answer(message);
                    }
                    else
                    {
                        Logger.Info("chat " + message.Chat.Id.ToString() + " " + gameObject.msgINobject.userAttempt.Name + " incorrect unswer");                        
                        gameObject.msgINobject = null;
                    }

                    if (gameObject.questionNumber.Count < 1 && !gameObject.game && gameObject.end)
                    {
                        gameObject.end = false;
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
            Game gameObject = (Game)gameChat[message.Chat.Id];

            gameObject.questionNumber.Remove(gameObject.questionNumber[0]);
            gameObject.num++;
            if (gameObject.questionNumber.Count != 0)
            {
                string temp = "Правильный ответ\n\n";
                gameObject.issuesObject = (IssuesClass)gameObject.issues[gameObject.questionNumber[0]];
                if (gameObject.questionNumber.Count > 1)
                    temp += "Следующий вопрос:\n\n";
                
                if (gameObject.issuesObject.TypeAnswer == 0)
                    gameObject.msgOUTobject = await SendIssuesReply(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, temp, message.MessageId);
                if (gameObject.issuesObject.TypeAnswer == 1)
                    gameObject.msgOUTobject = await SendIssuesButton(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, temp, message.MessageId);
            }
            else
            {
                gameObject.game = false;
                gameObject.end = true;
            }
            gameObject.msgINobject = null;
        }

        static async void EndGame(Telegram.Bot.Types.Message message)
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];

            Logger.Success("chat " + message.Chat.Id.ToString() + " end game");            
            string win = Statistics.GetStatistics(gameObject.messageOUTobject, config, message.Chat.Id, message.Chat.FirstName);
            gameObject.issues.Clear();
            await Bot.SendTextMessageAsync(message.Chat.Id, win);
            await Task.Delay(config.DeletionDelay);
            DeleteMsg(message);
            gameObject.num = 0;
        }

        static async void DeleteMsg(Telegram.Bot.Types.Message message)
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];

            Logger.Info("chat " + message.Chat.Id.ToString() + " start delete message");
            foreach (MessageOUT msgDel in gameObject.messageOUTobject)
            {
                if (msgDel != null)
                    try
                    {
                        await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("chat " + message.Chat.Id.ToString() + " " + e.Message);
                        Logger.Warn("chat " + message.Chat.Id.ToString() + " " + msgDel.MessageText);
                    }
            }

            foreach (MessageIN msgDel in gameObject.messageINobject)
            {
                if (msgDel != null)
                    try
                    {
                        await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("chat " + message.Chat.Id.ToString() + " " + e.Message); 
                        Logger.Warn("chat " + message.Chat.Id.ToString() + " " + msgDel.MessageText);
                    }
            }
            gameObject.messageOUTobject.Clear();
            gameObject.messageINobject.Clear();
            Logger.Info("chat " + message.Chat.Id.ToString() + " end delete message");
        }

        static async public void SendMsg(long chat_id, string text)
        {
            var msgTemp = await Bot.SendTextMessageAsync(chat_id, text);
            MessageOUT msgOUT = await SaveMsgOUT(msgTemp, 0);
            msgOUT = null;
        }
                
        static async public Task Insert(Telegram.Bot.Types.Message message)
        {
            string userInsertName = "Пользователь " + GetUserName(message) + ": ";
            insert = true;
            InsertOptions insertOptions = (InsertOptions)insertUser[message.Chat.Id];
            
            if (insertOptions.newQuestion == null)
                insertOptions.newQuestion = new IssuesClass();
            
            if (String.IsNullOrEmpty(insertOptions.newQuestion.QuestionText) && insertOptions.flag == -1)
            {
                Logger.Info(userInsertName + " добавляет новый вопрос");
                // config.RuleInsert
                insertOptions.flag = 0;
                await SendInsertMsg(message.Chat.Id, "Введите текст вопроса", insertOptions);
                return;
            }
            if (insertOptions.flag == 0)
            {
                if (!String.IsNullOrEmpty(message.Text))
                {
                    insertOptions.newQuestion.QuestionText = message.Text.Replace("\n", "@BR");
                    Logger.Info(userInsertName + "введенный вопрос: " + message.Text);
                    insertOptions.flag = 1;
                }
            }
            if (insertOptions.flag == 1)
            {
                insertOptions.flag = 2;
                await SendInsertMsg(message.Chat.Id, "Введите количество вариантов ответа (для реплая не более 5, для кнопок - не более 3)", insertOptions);
                return;
            }
            if (insertOptions.flag == 2)
            {
                bool res = int.TryParse(message.Text, out insertOptions.count);
                if (!res || (insertOptions.count > 5 || insertOptions.count < 1))
                {
                    insertOptions.flag = 2;
                    insertOptions.count = -1;
                    await SendInsertMsg(message.Chat.Id, "Введите количество вариантов ответа (максимум 5)", insertOptions);
                    return;
                }
                else
                {
                    Logger.Info(userInsertName + "количество вариантов ответа: " + message.Text);                    
                    insertOptions.flag = 3;                    
                }
            }

            if (insertOptions.flag == 3)
            {
                if (insertOptions.countQ <= insertOptions.count)
                {                   
                    switch (insertOptions.countQ)
                    {
                        case 1:
                            {
                                insertOptions.newQuestion.PossibleAnswer_1 = message.Text;
                                break;
                            }
                        case 2:
                            {
                                insertOptions.newQuestion.PossibleAnswer_2 = message.Text;
                                break;
                            }
                        case 3:
                            {
                                insertOptions.newQuestion.PossibleAnswer_3 = message.Text;
                                break;
                            }
                        case 4:
                            {
                                insertOptions.newQuestion.PossibleAnswer_4 = message.Text;
                                break;
                            }
                        case 5:
                            {
                                insertOptions.newQuestion.PossibleAnswer_5 = message.Text;
                                break;
                            }
                    }
                    Logger.Info(userInsertName + "введенный ответ: " + message.Text);
                    if (insertOptions.countQ < insertOptions.count)
                    {
                        insertOptions.flag = 3;
                        await SendInsertMsg(message.Chat.Id, "Введите " + (insertOptions.countQ + 1).ToString() + " вариант ответа", insertOptions);
                        insertOptions.countQ++;
                        return;
                    }
                    else
                    {
                        if (insertOptions.count != 1)
                        {
                            insertOptions.flag = 4;
                            await SendInsertMsg(message.Chat.Id, "Введите номер верного варианта ответа", insertOptions);
                            return;
                        }
                        else
                        {
                            insertOptions.flag = 6;
                            insertOptions.newQuestion.CorrectAnswer = insertOptions.newQuestion.PossibleAnswer_1;
                            insertOptions.newQuestion.TypeAnswer = 0;
                        }                     
                    }
                }                
            }

            if (insertOptions.flag == 4)
            {
                bool res = int.TryParse(message.Text, out int n);
                if (!res || (n > insertOptions.count) || (n < 1))
                {
                    insertOptions.flag = 4;
                    await SendInsertMsg(message.Chat.Id, "Введите номер верного варианта ответа", insertOptions);
                    return;
                }
                else
                {                    
                    insertOptions.newQuestion.CorrectAnswer = n.ToString();
                    Logger.Info(userInsertName + "верный вариант ответа: " + message.Text);
                    insertOptions.flag = 5;
                }
            }

            if (insertOptions.flag == 5 && insertOptions.count > 1)
            {
                insertOptions.flag = 6;
                await SendInsertMsg(message.Chat.Id, "Введите 0, если ответ через реплай и 1, если ответ через кнопки", insertOptions);
                return;
            }
            else
            if (insertOptions.flag == 5 && insertOptions.count == 1)
            {
                insertOptions.newQuestion.TypeAnswer = 0;
                Logger.Info(userInsertName + "тип ответа: реплай");
                insertOptions.flag = 7;
            }

            if (insertOptions.flag == 6)
            {
                bool res = int.TryParse(message.Text, out int n);
                if (res)
                {                    
                    insertOptions.newQuestion.TypeAnswer = n;
                    Logger.Info(userInsertName + "тип ответа: " + message.Text);
                    insertOptions.flag = 7;
                }
                else
                if (!res || (n != 0 || n != 1))
                {
                    insertOptions.flag = 6;
                    await SendInsertMsg(message.Chat.Id, "Введите 0, если ответ через реплай или 1, если ответ через кнопки", insertOptions);
                    return;
                }
            }

            if (insertOptions.flag == 7)
            {
                string txtQuest = "Проверьте правильнность введенных данных:\n\n" + insertOptions.newQuestion.QuestionText;
                txtQuest += !String.IsNullOrEmpty(insertOptions.newQuestion.PossibleAnswer_1) ? "\n\nВарианты ответов:\n1 - " +
                    insertOptions.newQuestion.PossibleAnswer_1 : "";
                txtQuest += !String.IsNullOrEmpty(insertOptions.newQuestion.PossibleAnswer_2) ? "\n2 - " + insertOptions.newQuestion.PossibleAnswer_2 : "";
                txtQuest += !String.IsNullOrEmpty(insertOptions.newQuestion.PossibleAnswer_3) ? "\n3 - " + insertOptions.newQuestion.PossibleAnswer_3 : "";
                txtQuest += !String.IsNullOrEmpty(insertOptions.newQuestion.PossibleAnswer_4) ? "\n4 - " + insertOptions.newQuestion.PossibleAnswer_4 : "";
                txtQuest += !String.IsNullOrEmpty(insertOptions.newQuestion.PossibleAnswer_5) ? "\n5 - " + insertOptions.newQuestion.PossibleAnswer_5 : "";
                txtQuest += "\n\nВерный ответ - " + insertOptions.newQuestion.CorrectAnswer;
                txtQuest += "\n\nОтвет с помощью ";
                txtQuest += insertOptions.newQuestion.TypeAnswer == 0 ? "реплая" : "кнопок";
                txtQuest += "\n\nЕсли все верно - введите 1, иначе 0";

                Logger.Info(userInsertName + txtQuest);

                insertOptions.flag = 8;
                await SendInsertMsg(message.Chat.Id, txtQuest, insertOptions);
                return;
            }

            if (insertOptions.flag == 8 && message.Text == "1")
            {
                Logger.Info(userInsertName + "insert newQuestion in base");
                if (config.InsertIssues(insertOptions.newQuestion, message.From.Id, GetUserName(message)))
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ваш вопрос добавлен");
                    Logger.Info(userInsertName + "insert true");
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ошибка при добавлении вопроса");
                    Logger.Info(userInsertName + "insert false");
                }
                insert = false;
                insertUser.Remove(message.Chat.Id);
            }
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Редактирования пока нет, вводите все заново, раз так!");
                Logger.Info(userInsertName + "incorrect");
                insert = false;
                insertUser.Remove(message.Chat.Id);
            }
        }

        static async Task SendInsertMsg(long chatId, string text, InsertOptions insertOptions)
        {
            await Bot.SendTextMessageAsync(chatId, text);
            insertUser[chatId] = insertOptions;
        }
    }
}