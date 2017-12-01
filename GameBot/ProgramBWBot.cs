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

/*
 * Links 'tg://user?id=<user_id>' can be used to mention a user by their id without using a username.
 * */

namespace BotGame
{
    partial class Program
    {
        private static Hashtable gameChat = new Hashtable();
        private static Hashtable insertUser = new Hashtable();
        private static Hashtable deleteUser = new Hashtable();

        static bool StartGame(Telegram.Bot.Types.Message message)
        {
            if ((!gameChat.ContainsKey(message.Chat.Id)) && 
                (config.ADMIN.Contains(message.From.Id.ToString())
                || (message.Chat.Type == ChatType.Private)))
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
            TelegramBotClient Bot, long chatId, string num, 
            IssuesClass issues, string textStart = "", int replayMsgId = 0)
        {
            Game gameObject = (Game)gameChat[chatId];
            Logger.Info(issues.QuestionText);
            string txtQuest = "<b>" + issues.QuestionText + "</b>";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? "\n\nВарианты ответов:\n1: " + 
                issues.PossibleAnswer_1 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? "\n2: " + issues.PossibleAnswer_2 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? "\n3: " + issues.PossibleAnswer_3 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_4) ? "\n4: " + issues.PossibleAnswer_4 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_5) ? "\n5: " + issues.PossibleAnswer_5 : "";

            Telegram.Bot.Types.Message msg = new Telegram.Bot.Types.Message();

            string textMsg = textStart + "Вопрос " + num + "\n" + txtQuest;

            if (replayMsgId == 0)
                try
                {
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Html);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException eqw)
                {
                    textMsg = textMsg.Replace("<b>", "");
                    textMsg = textMsg.Replace("</b>", "");
                    Logger.Warn(eqw.Message);
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Default);
                }
            else
                try
                {
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Html,
                        replyToMessageId: gameObject.msgINobject.MessageId);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException eqw)
                {
                    textMsg = textMsg.Replace("<b>", "");
                    textMsg = textMsg.Replace("</b>", "");
                    Logger.Warn(eqw.Message);
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Default,
                        replyToMessageId: gameObject.msgINobject.MessageId);
                }

            if (!String.IsNullOrEmpty(textStart))
                msg.Text = "Вопрос " + num + "\n" + txtQuest;
            MessageOUT msgOUTtemp = await SaveMsgOUT(msg, issues.Id);
            Logger.Info("chat " + chatId.ToString() + " " + num.ToString() + " question submitted");
            return msgOUTtemp;
        }

        static async Task<MessageOUT> SendIssuesButton(
            TelegramBotClient Bot, long chatId, string num, 
            IssuesClass issues, string textStart = "", int replayMsgId = 0)
        {
            Game gameObject = (Game)gameChat[chatId];
            Logger.Info(issues.QuestionText);
            string btn1 = !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? issues.PossibleAnswer_1 : "";
            string btn2 = !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? issues.PossibleAnswer_2 : "";
            string btn3 = !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? issues.PossibleAnswer_3 : "";

            var replyMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
            {
                new InlineKeyboardCallbackButton(btn1, "1"),
                new InlineKeyboardCallbackButton(btn2, "2"),
                new InlineKeyboardCallbackButton(btn3, "3"),
            });

            Telegram.Bot.Types.Message msg = new Telegram.Bot.Types.Message();

            string textMsg = textStart + "Вопрос " + num + "\n<b>" + issues.QuestionText + "</b>";

            if (replayMsgId == 0)
                try
                {
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException eqw)
                {
                    textMsg = textMsg.Replace("<b>", "");
                    textMsg = textMsg.Replace("</b>", "");
                    Logger.Warn(eqw.Message);
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Default,
                       replyMarkup: replyMarkup);
                }
            else
                try
                {
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Html,
                        replyMarkup: replyMarkup, replyToMessageId: gameObject.msgINobject.MessageId);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException eqw)
                {
                    textMsg = textMsg.Replace("<b>", "");
                    textMsg = textMsg.Replace("</b>", "");
                    Logger.Warn(eqw.Message);
                    msg = await Bot.SendTextMessageAsync(chatId, textMsg, parseMode: ParseMode.Default,
                        replyMarkup: replyMarkup, replyToMessageId: gameObject.msgINobject.MessageId);
                }
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
            if (gameObject != null)
                gameObject.messageOUTobject.Add(msgOUTtemp);
            return msgOUTtemp;
        }

        public static string GetUserName(Telegram.Bot.Types.Message message)
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

        private static Game SelectIssuesGame(Telegram.Bot.Types.Message message)
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];
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
            return gameObject;
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
                    if (gameObject == null)
                        gameObject = new Game();

                    if ((message.Text.StartsWith("/count")) && config.ADMIN.Contains(message.From.Id.ToString()))
                    {
                        try
                        {
                            await SendMsg(message.Chat.Id, "Количество вопросов в базе: " + config.CountIssue());
                            Logger.Info("send count issues in base in chat: " + message.Chat.Title);
                        }
                        catch
                        {
                            Logger.Warn("not send count issues in base in chat: " + message.Chat.Title);
                        }
                    }

                    if ((message.Text.StartsWith("/win")) && config.ADMIN.Contains(message.From.Id.ToString()))
                    {
                        try
                        {
                            await SendMsg(message.Chat.Id, "Рейтинг чата:\n\n" + config.SelectWin(message.Chat.Id));
                            Logger.Info("send win in chat: " + message.Chat.Title);
                        }
                        catch (Exception ew)
                        {
                            Logger.Warn("not send win in chat: " + message.Chat.Title);
                            Logger.Warn(ew.Message);
                        }
                    }

                    InsertOptions insertOptions = (InsertOptions)insertUser[message.Chat.Id];
                    if ((message.Text.StartsWith("/insert")) || (insertOptions != null))
                        if (config.ADMIN.Contains(message.From.Id.ToString()))
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
                                await Insert.InsertQuestion(message, insertUser, Bot, config);
                                //return;
                            }

                    DeleteOptions deleteOptions = (DeleteOptions)deleteUser[message.From.Id];
                    if ((message.Text.StartsWith("/delete")) || (deleteOptions != null))
                        if (config.ADMIN.Contains(message.From.Id.ToString()))
                            if (message.Chat.Type == ChatType.Private)
                            {
                                await Delete.DeleteQuestion(message, deleteUser, config, Bot);
                                //return;
                            }

                    string textStart = "";
                    if (message.Text == @"/newgame")
                        if (StartGame(message))
                        {
                            var m = await SaveMsgIn(message);
                            Logger.Success("chat " + message.Chat.Title + " start game");
                            gameObject = SelectIssuesGame(message);
                            Logger.Info("chat " + message.Chat.Title + " всего вопросов: " + gameObject.questionNumber.Count.ToString());
                            string count = config.SelectCountGame(message.Chat.Id);
                            textStart += "Игра №" + count + " началась!\nВсего вопросов: " + gameObject.questionNumber.Count.ToString() + "\n\n";
                            gameObject.game = true;
                        }
                    if (gameObject.game)
                    {
                        // получаем вопрос
                        gameObject.issuesObject = (IssuesClass)gameObject.issues[gameObject.questionNumber[0]];
                        
                        if (gameObject.issuesObject.TypeAnswer == IssuesClass.TYPE_ANSWER_REPLY)
                        {
                            if (gameObject.msgOUTobject is null)
                            {
                                gameObject.num++;
                                gameObject.msgOUTobject = await SendIssuesReply(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, textStart);
                                textStart = "";
                            }
                            else
                            {
                                if (message.ReplyToMessage != null && message.ReplyToMessage.MessageId == gameObject.msgOUTobject.MessageId)
                                {
                                    gameObject.msgINobject = await SaveMsgIn(message);

                                    if (gameObject.msgINobject.ReplayToUserId == config.IDBOT && gameObject.msgOUTobject.MessageId == gameObject.msgINobject.ReplayToMessageId)
                                    {
                                        gameObject.msgOUTobject.AttemptsAnswers++;
                                        if (gameObject.issuesObject.CorrectAnswer.ToLower().Trim() == gameObject.msgINobject.MessageText.ToLower().Trim())
                                        {
                                            Logger.Info("chat " + message.Chat.Title + " " + gameObject.msgINobject.userAttempt.Name + " correct answer");
                                            gameObject.msgOUTobject.AnswerDate = gameObject.msgINobject.MmessageDate;
                                            gameObject.msgOUTobject.userWin = gameObject.msgINobject.userAttempt;
                                            try
                                            {
                                                await Answer(message);
                                            }
                                            catch (Exception e1)
                                            {
                                                Logger.Error(e1.Message);
                                            }
                                        }
                                        else
                                        {
                                            Logger.Info("chat " + message.Chat.Title + " " + gameObject.msgINobject.userAttempt.Name + " incorrect answer");
                                            gameObject.msgINobject = null;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        if ((gameObject.issuesObject.TypeAnswer == IssuesClass.TYPE_ANSWER_BUTTON) 
                            && (gameObject.msgOUTobject is null))
                        {
                            gameObject.num++;
                            gameObject.msgOUTobject = await SendIssuesButton(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, textStart);
                            textStart = "";
                        }
                        else 
                        if ((gameObject.issuesObject.TypeAnswer == IssuesClass.TYPE_ANSWER_BUTTON))
                        {
                            if (message.ReplyToMessage != null)
                            {
                                gameObject.msgINobject = await SaveMsgIn(message);
                                gameObject.msgOUTobject.AttemptsAnswers++;
                            }
                        }
                    }
                    
                    if (gameObject.questionNumber.Count < 1 && !gameObject.game && gameObject.end)
                    {
                        gameObject.end = false;
                        await EndGame(message);
                    }
                };
                
                Bot.OnCallbackQuery += async (object sc, CallbackQueryEventArgs ev) =>
                {
                    var message = ev.CallbackQuery.Message;
                    Game gameObject = (Game)gameChat[message.Chat.Id];
                    if ((gameObject != null) && gameObject.game)
                    {
                        await OnCallbackQueryGame(message, ev, gameObject);
                        return;
                    }

                    try
                    {
                        await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ert)
                    {
                        Logger.Warn("Ошибка при редактировании кнопок:" + ert.Message);
                    }

                    InsertOptions insertOptions = (InsertOptions)insertUser[message.Chat.Id];
                    if (insertOptions != null)
                    {
                        insertOptions.btnText = ev.CallbackQuery.Data;
                        try
                        {
                            try
                            {
                                await Bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, message.Text,
                                    parseMode: ParseMode.Html, replyMarkup: null);
                            }
                            catch (Telegram.Bot.Exceptions.ApiRequestException ewq)
                            {
                                Logger.Warn(ewq.Message);
                                await Bot.EditMessageTextAsync(message.Chat.Id, message.MessageId, message.Text,
                                parseMode: ParseMode.Default, replyMarkup: null);
                            }
                        }
                        catch (Telegram.Bot.Exceptions.ApiRequestException ert)
                        {
                            Logger.Warn("Ошибка при редактировании кнопок:" + ert.Message);
                        }
                        await Insert.InsertQuestion(message, insertUser, Bot, config);
                        return;
                    }
                };
                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Logger.Error(ex.Message);
            }
        }

        static async Task OnCallbackQueryGame(Telegram.Bot.Types.Message message, CallbackQueryEventArgs ev, Game gameObject)
        {
            message.From = ev.CallbackQuery.From;
            gameObject.msgINobject = await SaveMsgIn(message);
            gameObject.msgOUTobject.AttemptsAnswers++;
            if (gameObject.issuesObject.CorrectAnswer.ToLower().Trim() == ev.CallbackQuery.Data.ToLower().Trim())
            {
                try
                {
                    await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ert)
                {
                    Logger.Warn("Ошибка при редактировании кнопок:" + ert.Message);
                }

                try
                {
                    try
                    {
                        await Bot.EditMessageTextAsync(gameObject.msgOUTobject.ChatId, gameObject.msgOUTobject.MessageId, gameObject.msgOUTobject.MessageText,
                            parseMode: ParseMode.Html, replyMarkup: null);
                    }
                    catch (Telegram.Bot.Exceptions.ApiRequestException ewq)
                    {
                        Logger.Warn(ewq.Message);
                        await Bot.EditMessageTextAsync(gameObject.msgOUTobject.ChatId, gameObject.msgOUTobject.MessageId, gameObject.msgOUTobject.MessageText,
                        parseMode: ParseMode.Default, replyMarkup: null);
                    }
                }
                catch (Telegram.Bot.Exceptions.ApiRequestException ert)
                {
                    Logger.Warn("Ошибка при редактировании кнопок:" + ert.Message);
                }
                gameObject.msgOUTobject.userWin = gameObject.msgINobject.userAttempt;
                gameObject.msgOUTobject.AnswerDate = gameObject.msgINobject.MmessageDate;
                string name = String.IsNullOrEmpty(ev.CallbackQuery.From.LastName) ? ev.CallbackQuery.From.FirstName : ev.CallbackQuery.From.FirstName
                + " " + ev.CallbackQuery.From.LastName;
                string text = " '" + gameObject.issuesObject.CorrectAnswer + "' получен от " + name;
                try
                {
                    await Answer(message, null, text);
                }
                catch (Exception e1)
                {
                    Logger.Error(e1.Message);
                }
            }
            else
            {
                await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id, "'" + ev.CallbackQuery.Data + "' неверный ответ!", false);
                Logger.Info("chat " + message.Chat.Id.ToString() + " " + gameObject.msgINobject.userAttempt.Name 
                    + " incorrect answer");
                gameObject.msgINobject = null;
            }

            if (gameObject.questionNumber.Count < 1 && !gameObject.game && gameObject.end)
            {
                gameObject.end = false;
                EndGame(message);
            }
        }

        static async Task Answer(Telegram.Bot.Types.Message message, string button = "", string text = "")
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];
            gameObject.questionNumber.Remove(gameObject.questionNumber[0]);
            gameObject.num++;
            if (gameObject.questionNumber.Count != 0)
            {
                string temp = "Правильный ответ";
                temp += !String.IsNullOrEmpty(text) ? text + "\n\n" : "\n\n";
                gameObject.issuesObject = (IssuesClass)gameObject.issues[gameObject.questionNumber[0]];
                if (gameObject.questionNumber.Count > 1)
                    temp += "Следующий вопрос:\n\n";
                
                if (gameObject.issuesObject.TypeAnswer == IssuesClass.TYPE_ANSWER_REPLY)
                    gameObject.msgOUTobject = await SendIssuesReply(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, temp, message.MessageId);
                if (gameObject.issuesObject.TypeAnswer == IssuesClass.TYPE_ANSWER_BUTTON)
                    gameObject.msgOUTobject = await SendIssuesButton(Bot, message.Chat.Id, gameObject.num.ToString(), gameObject.issuesObject, temp, message.MessageId);
            }
            else
            {
                gameObject.game = false;
                gameObject.end = true;
            }
            gameObject.msgINobject = null;
        }

        static async Task EndGame(Telegram.Bot.Types.Message message)
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];
            Logger.Success("chat " + message.Chat.Title + " end game");
            string win = Statistics.GetStatistics(gameObject.messageOUTobject, config, message.Chat.Id, message.Chat.FirstName);
            gameObject.issues.Clear();
            await Bot.SendTextMessageAsync(message.Chat.Id, win);
            await Task.Delay(config.DeletionDelay);
            await DeleteMsg(message);
            gameObject.num = 0;
            gameChat.Remove(message.Chat.Id);
        }

        static async Task DeleteMsg(Telegram.Bot.Types.Message message)
        {
            Game gameObject = (Game)gameChat[message.Chat.Id];
            Logger.Info("chat " + message.Chat.Title + " start delete message");
            foreach (MessageOUT msgDel in gameObject.messageOUTobject)
            {
                if (msgDel != null)
                    try
                    {
                        await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
                    }
                    catch (Exception e)
                    {
                        Logger.Error("chat " + message.Chat.Title + " " + e.Message);
                        Logger.Warn("chat " + message.Chat.Title + " " + msgDel.MessageText);
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
                        Logger.Error("chat " + message.Chat.Title + " " + e.Message); 
                        Logger.Warn("chat " + message.Chat.Title + " " + msgDel.MessageText);
                    }
            }
            gameObject.messageOUTobject.Clear();
            gameObject.messageINobject.Clear();
            Logger.Info("chat " + message.Chat.Title + " end delete message");
        }

        public static async Task SendMsg(long chatId, string text)
        {
            Telegram.Bot.Types.Message msgTemp = new Telegram.Bot.Types.Message();
            try
            {
                msgTemp = await Bot.SendTextMessageAsync(chatId, text, parseMode: ParseMode.Html);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ewq)
            {
                Logger.Warn(ewq.Message);
                msgTemp = await Bot.SendTextMessageAsync(chatId, text, parseMode: ParseMode.Default);
            }
            MessageOUT msgOUT = await SaveMsgOUT(msgTemp, 0);
            msgOUT = null;
        }        
    }
} 
