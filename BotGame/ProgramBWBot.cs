using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;

namespace BotGame
{
    partial class Program
    {
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

        static async Task<MessageOUT> SendIssuesReply(TelegramBotClient Bot, long chatId, string num, string questionText)
        {
            var msg = await Bot.SendTextMessageAsync(chatId, "Вопрос " + num + "\n" + questionText);
            MessageOUT msgOUT = new MessageOUT
            {
                ChatId = msg.Chat.Id.ToString(),
                MessageId = msg.MessageId.ToString(),
                MessageText = msg.Text,
                MmessageDate = msg.Date
            };
            Logger.Info(num.ToString() + " question submitted");
            return msgOUT;
        }

        static async Task<MessageOUT> SendIssuesButton(TelegramBotClient Bot, long chatId, string num, string questionText)
        {
            // сменить варианты ответов на кнопки
            var msg = await Bot.SendTextMessageAsync(chatId, "Вопрос " + num + "\n" + questionText);
            MessageOUT msgOUT = new MessageOUT
            {
                ChatId = msg.Chat.Id.ToString(),
                MessageId = msg.MessageId.ToString(),
                MessageText = msg.Text,
                MmessageDate = msg.Date
            };
            Logger.Info(num.ToString() + " question submitted");
            return msgOUT;
        }

        //static async Task CheckCorrectAnswer(TelegramBotClient Bot, )
        //{

        //}

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
                bool answer = false;

                Bot.OnUpdate += async (object su, Telegram.Bot.Args.UpdateEventArgs evu) =>
                {
                    // bool resultReplay = false;
                    if (evu.Update.CallbackQuery != null || evu.Update.InlineQuery != null)
                        return;

                    var update = evu.Update;
                    var message = update.Message;

                    if (message == null)
                        return;

                    if ((message.Type == Telegram.Bot.Types.Enums.MessageType.TextMessage) 
                        && (message.Text.StartsWith("/insert") || message.Text.StartsWith("/insert@ucs13bot")))
                    {
                        // добавление вопросов                        
                    }

                    if (GetActivity())
                    {                        
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
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Внимание, начинается игра!\nВсего вопросов: " + questionNumber.Count.ToString());
                        Logger.Success("start game");
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
                            if (msgOUT is null)
                            {
                                num++;
                                string txtQuest = issues.QuestionText;
                                txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? "\n\nВарианты ответов:\n" + issues.PossibleAnswer_1 : "";
                                txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? "\n" + issues.PossibleAnswer_2 : "";
                                txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? "\n" + issues.PossibleAnswer_3 : "";

                                msgOUT = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), txtQuest);                                
                            }
                            // проверка ответа
                            if (msgOUT != null)
                            {
                                if (message.ReplyToMessage != null)
                                {
                                    msgIN = new MessageIN
                                    {
                                        MessageId = message.MessageId.ToString(),
                                        MessageText = message.Text,
                                        UserName = String.IsNullOrEmpty(message.From.LastName) ? message.From.FirstName : message.From.FirstName + " " + message.From.LastName,
                                        ReplayToMessageId = message.ReplyToMessage.MessageId.ToString(),
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
                                }// проверка ответа                                
                            }
                        }// если ответ через реплай
                        else
                        // если ответ через кнопки
                        if (issues.TypeAnswer == 1)
                        {

                        }// если ответ через кнопки

                        if (answer)
                        {
                            await Bot.SendTextMessageAsync(msgOUT.ChatId, "Правильный ответ!", replyToMessageId: Convert.ToInt32(msgIN.MessageId));
                            messageOUT.Add(msgOUT);
                            messageIN.Add(msgIN);
                            msgOUT = null;
                            questionNumber.Remove(questionNumber[0]);
                            num++;
                            if (questionNumber.Count != 0)
                            {
                                issues = (IssuesClass)config.issues[questionNumber[0]];

                                string txtQuest = issues.QuestionText;
                                txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_1) ? "\n" + issues.PossibleAnswer_1 : "";
                                txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_2) ? "\n" + issues.PossibleAnswer_2 : "";
                                txtQuest += !String.IsNullOrEmpty(issues.PossibleAnswer_3) ? "\n" + issues.PossibleAnswer_3 : "";

                                msgOUT = await SendIssuesReply(Bot, message.Chat.Id, num.ToString(), txtQuest);
                            }
                            else
                            {
                                game = 0;
                                end = true;
                            }
                            msgIN = null;
                            answer = false;
                        }                       
                    }

                    if (questionNumber.Count < 1 && game == 0 && end)
                    {
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Внимание, игра закончена!");
                        Logger.Success("end game");
                        config.issues = null; // очищаем список вопросов в конце игры
                        end = false;
                        // получение статистики
                    }
                };
                Bot.StartReceiving();
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException ex)
            {
                Logger.Error(ex.Message);
            }
        }
    }
}