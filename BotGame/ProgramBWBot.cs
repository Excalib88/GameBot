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
        static List<MessageOUT> messageOUT = new List<MessageOUT>();
        static List<MessageIN> messageIN = new List<MessageIN>();

        static bool game = false;

        static bool StartGame(string text)
        {            
            if ((text == @"/newgame" || text == @"/newgame@ucs13bot") && !game)
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
            TelegramBotClient Bot, long chatId, string num, IssuesClass issues)
        {
            string txtQuest = issues.QuestionText;
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? "\n\nВарианты ответов:\n" + 
                issues.PossibleAnswer_1 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? "\n" + issues.PossibleAnswer_2 : "";
            txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? "\n" + issues.PossibleAnswer_3 : "";

            var msg = await Bot.SendTextMessageAsync(chatId, "Вопрос " + num + "\n" + txtQuest);
            MessageOUT msgOUT = new MessageOUT
            {
                ChatId = msg.Chat.Id,
                MessageId = msg.MessageId,
                MessageText = msg.Text,
                MmessageDate = msg.Date
            };
            Logger.Info(num.ToString() + " question submitted");
            return msgOUT;
        }

        static async Task<MessageOUT> SendIssuesButton(
            TelegramBotClient Bot, long chatId, string num, IssuesClass issues)
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

            var msg = await Bot.SendTextMessageAsync(chatId, "Вопрос " + num + "\n" + issues.QuestionText,
                replyMarkup: replyMarkup);

            MessageOUT msgOUT = new MessageOUT
            {
                ChatId = msg.Chat.Id,
                MessageId = msg.MessageId,
                MessageText = msg.Text,
                MmessageDate = msg.Date
            };            

            Logger.Info(num.ToString() + " question submitted");
            return msgOUT;
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
                int num = 0;
                bool end = false;
                bool answer = false;
                IssuesClass issues = null;

                Bot.OnUpdate += async (object su, UpdateEventArgs evu) =>
                {
                    // bool resultReplay = false;
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null)
                        return;
                                        
                    var message = evu.Update.Message;

                    if (message == null || message.Type != MessageType.TextMessage)
                        return;

                    if ((message.Text.StartsWith("/insert") || message.Text.StartsWith("/insert@ucs13bot"))
                        && (message.Chat.Type == ChatType.Private))
                    {
                        // добавление вопросов                        
                    }

                    if (StartGame(message.Text))
                    {
                        msgIN = new MessageIN
                        {
                            ChatId = message.Chat.Id,
                            MessageId = message.MessageId,
                            MessageText = message.Text,
                            UserName = String.IsNullOrEmpty(message.From.LastName) ? message.From.FirstName : message.From.FirstName 
                                + " " + message.From.LastName
                        };
                        messageIN.Add(msgIN);
                        msgIN = null;

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
                        var msgTemp = await Bot.SendTextMessageAsync(message.Chat.Id, 
                            "Игра началась!\nВсего вопросов: " + questionNumber.Count.ToString());
                        msgOUT = new MessageOUT
                        {
                            ChatId = msgTemp.Chat.Id,
                            MessageId = msgTemp.MessageId,
                            MessageText = msgTemp.Text,
                            MmessageDate = msgTemp.Date
                        };
                        messageOUT.Add(msgOUT);
                        msgOUT = null;
                        game = true;                        
                        await Task.Delay(3000);
                    }

                    if (game)
                    {
                        // получаем вопрос
                        issues = (IssuesClass)config.issues[questionNumber[0]];

                        // если ответ через реплай
                        if (issues.TypeAnswer == 0)
                        {
                            if (msgOUT is null)
                            {
                                num++;                                
                                msgOUT = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), issues);                                
                            }
                            else
                            {
                                if (message.ReplyToMessage != null)
                                {
                                    msgIN = new MessageIN
                                    {
                                        ChatId = message.Chat.Id,
                                        MessageId = message.MessageId,
                                        MessageText = message.Text,
                                        UserName = String.IsNullOrEmpty(message.From.LastName) ? message.From.FirstName : message.From.FirstName 
                                            + " " + message.From.LastName,
                                        ReplayToMessageId = message.ReplyToMessage.MessageId,
                                        ReplayToMessageText = message.ReplyToMessage.Text,
                                        ReplayToUserId = message.ReplyToMessage.From.Id.ToString()
                                    };

                                    if (msgIN.ReplayToUserId == config.IDBOT && msgOUT.MessageId == msgIN.ReplayToMessageId)
                                    {                                       
                                        if (issues.CorrectAnswer.ToLower().Trim() == msgIN.MessageText.ToLower().Trim())
                                        {
                                            // принимаем ответ как верный
                                            answer = true;
                                            Logger.Success(msgIN.UserName + " correct unswer");
                                        }
                                        else
                                        {
                                            // считаем ответ как попытку ответить
                                            Logger.Info(msgIN.UserName + " incorrect unswer");
                                            msgOUT.AttemptsAnswers++;
                                            msgIN = null;
                                        }                                        
                                    }
                                }                             
                            }
                        }// если ответ через реплай
                        else
                        // если ответ через кнопки
                        if (issues.TypeAnswer == 1)
                        {
                            if (msgOUT is null)
                            {
                                num++;
                                msgOUT = await SendIssuesButton(Bot, message.Chat.Id, num.ToString(), issues);
                            }                       
                        }// если ответ через кнопки

                        if (answer)
                        {
                            var msgTemp = await Bot.SendTextMessageAsync(msgOUT.ChatId, "Правильный ответ!", 
                                replyToMessageId: msgIN.MessageId);
                            messageOUT.Add(msgOUT);
                            messageIN.Add(msgIN);
                            msgOUT = new MessageOUT
                            {
                                ChatId = msgTemp.Chat.Id,
                                MessageId = msgTemp.MessageId,
                                MessageText = msgTemp.Text,
                                MmessageDate = msgTemp.Date
                            };
                            messageOUT.Add(msgOUT);                            
                            msgOUT = null;
                            questionNumber.Remove(questionNumber[0]);
                            num++;
                            if (questionNumber.Count != 0)
                            {
                                issues = (IssuesClass)config.issues[questionNumber[0]];      
                                if (issues.TypeAnswer == 0)
                                    msgOUT = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), issues);
                                if (issues.TypeAnswer == 1)
                                    msgOUT = await SendIssuesButton(Bot, message.Chat.Id, num.ToString(), issues);
                            }
                            else
                            {
                                game = false;
                                end = true;
                            }
                            msgIN = null;
                            answer = false;
                        }                       
                    }

                    if (questionNumber.Count < 1 && !game && end)
                    {
                        var msgTemp = await Bot.SendTextMessageAsync(message.Chat.Id, "Конец игры");
                        msgOUT = new MessageOUT
                        {
                            ChatId = msgTemp.Chat.Id,
                            MessageId = msgTemp.MessageId,
                            MessageText = msgTemp.Text,
                            MmessageDate = msgTemp.Date
                        };
                        messageOUT.Add(msgOUT);
                        msgOUT = null;
                        Logger.Success("end game");
                        config.issues = null; // очищаем список вопросов в конце игры
                        end = false;
                        // получение статистики
                        DeleteMsg(Bot);
                    }
                };
                
                Bot.OnCallbackQuery += async (object sc, CallbackQueryEventArgs ev) =>
                {
                    var message = ev.CallbackQuery.Message;                    

                    if (issues.CorrectAnswer.ToLower().Trim() == ev.CallbackQuery.Data.ToLower().Trim())
                    {
                        await Bot.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
                        await Bot.EditMessageTextAsync(msgOUT.ChatId, msgOUT.MessageId, msgOUT.MessageText, 
                            parseMode: ParseMode.Default, replyMarkup: null);

                        string UserName = String.IsNullOrEmpty(message.From.LastName) ? ev.CallbackQuery.From.FirstName : ev.CallbackQuery.From.FirstName 
                        + " " + ev.CallbackQuery.From.LastName;
                        var msgTemp = await Bot.SendTextMessageAsync(msgOUT.ChatId, "Правильный ответ '" + 
                            issues.CorrectAnswer + "' получен от " + UserName);
                        messageOUT.Add(msgOUT);
                        messageIN.Add(msgIN);
                        msgOUT = new MessageOUT
                        {
                            ChatId = msgTemp.Chat.Id,
                            MessageId = msgTemp.MessageId,
                            MessageText = msgTemp.Text,
                            MmessageDate = msgTemp.Date
                        };
                        messageOUT.Add(msgOUT);                        
                        msgOUT = null;
                        questionNumber.Remove(questionNumber[0]);
                        num++;
                        if (questionNumber.Count != 0)
                        {
                            issues = (IssuesClass)config.issues[questionNumber[0]];
                            if (issues.TypeAnswer == 0)
                                msgOUT = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), issues);
                            if (issues.TypeAnswer == 1)
                                msgOUT = await SendIssuesButton(Bot, message.Chat.Id, num.ToString(), issues);
                        }
                        else
                        {
                            game = false;
                            end = true;
                        }
                        msgIN = null;
                        answer = false;
                    }

                    if (questionNumber.Count < 1 && !game && end)
                    {
                        var msgTemp = await Bot.SendTextMessageAsync(message.Chat.Id, "Конец игры");
                        msgOUT = new MessageOUT
                        {
                            ChatId = msgTemp.Chat.Id,
                            MessageId = msgTemp.MessageId,
                            MessageText = msgTemp.Text,
                            MmessageDate = msgTemp.Date
                        };
                        messageOUT.Add(msgOUT);
                        msgOUT = null;
                        Logger.Success("end game");
                        config.issues = null; // очищаем список вопросов в конце игры
                        end = false;
                        // получение статистики
                        DeleteMsg(Bot);
                    }
                };

                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Logger.Error(ex.Message);
            }            
        }

        static async void DeleteMsg(TelegramBotClient Bot)
        {
            foreach (MessageOUT msgDel in messageOUT)
            {
                if (msgDel != null)
                    await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
            }

            foreach (MessageIN msgDel in messageIN)
            {
                if (msgDel != null)
                    await Bot.DeleteMessageAsync(msgDel.ChatId, msgDel.MessageId);
            }
        }
    }
}