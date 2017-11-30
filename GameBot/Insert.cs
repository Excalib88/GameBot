using System;
using System.Collections;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineKeyboardButtons;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotGame
{
    public static class Insert
    {
        static async public Task SendMsgKeyboardInsert(long chatId, string text,
            string[] btnText, InsertOptions insertOptions, TelegramBotClient Bot, Hashtable insertUser)
        {
            InlineKeyboardCallbackButton[] inl = new InlineKeyboardCallbackButton[btnText.Length];
            for (int i = 0; i < btnText.Length; i++)
            {
                inl[i] = new InlineKeyboardCallbackButton(btnText[i], (i + 1).ToString());
            }

            InlineKeyboardButton[] InlButton = new InlineKeyboardButton[btnText.Length];

            for (int i = 0; i < btnText.Length; i++)
            {
                InlButton[i] = inl[i];
            }

            var replyMarkup = new InlineKeyboardMarkup(InlButton);

            try
            {
                await Bot.SendTextMessageAsync(chatId, text, parseMode: ParseMode.Html,
                    replyMarkup: replyMarkup);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException eqw)
            {
                Logger.Warn(eqw.Message);
                await Bot.SendTextMessageAsync(chatId, text, parseMode: ParseMode.Default,
                   replyMarkup: replyMarkup);
            }
            insertUser[chatId] = insertOptions;
        }

        static async public Task InsertQuestion(Telegram.Bot.Types.Message message, Hashtable insertUser,
            TelegramBotClient Bot, ConfigSQL config)
        {
            string userInsertName = "Пользователь " + Program.GetUserName(message) + ": ";

            InsertOptions insertOptions = (InsertOptions)insertUser[message.Chat.Id];
            insertOptions.insert = true;
            if (insertOptions.newQuestion == null)
                insertOptions.newQuestion = new IssuesClass();
            if (String.IsNullOrEmpty(insertOptions.newQuestion.QuestionText)
                && insertOptions.flag == InsertOptions.INSERT_FALSE)
            {
                Logger.Info(userInsertName + "добавляет новый вопрос");
                // config.RuleInsert
                insertOptions.flag = InsertOptions.INSERT_START;
                // todo: или пришлите картинку с вопросом
                await SendInsertMsg(message.Chat.Id, "Введите текст вопроса", insertOptions, insertUser, Bot);
                return;
            }
            if (insertOptions.flag == InsertOptions.INSERT_START)
            {
                if (!String.IsNullOrEmpty(message.Text))
                {
                    insertOptions.newQuestion.QuestionText = message.Text.Replace("\n", "@BR");
                    Logger.Info(userInsertName + "введенный вопрос: " + message.Text);
                    insertOptions.flag = InsertOptions.INSERT_COUNT_ANSWER;
                }
            }
            if (insertOptions.flag == InsertOptions.INSERT_COUNT_ANSWER)
            {
                insertOptions.flag = InsertOptions.INSERT_CORRECT_COUNT_ANSWER;
                await SendMsgKeyboardInsert(message.Chat.Id, "Выберите количество вариантов ответа.\nДля реплая не более 5, для кнопок - не более 3",
                    new string[] { "1", "2", "3", "4", "5" }, insertOptions, Bot, insertUser);
                return;
            }
            if (insertOptions.flag == InsertOptions.INSERT_CORRECT_COUNT_ANSWER)
            {
                bool res = int.TryParse(insertOptions.btnText, out insertOptions.count);
                if (!res || (insertOptions.count > 5 || insertOptions.count < 1))
                {
                    insertOptions.flag = InsertOptions.INSERT_CORRECT_COUNT_ANSWER;
                    insertOptions.count = InsertOptions.INSERT_FALSE;
                    return;
                }
                else
                {
                    Logger.Info(userInsertName + "количество вариантов ответа: " + insertOptions.btnText);
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Выбрано: " + insertOptions.btnText);
                    insertOptions.flag = InsertOptions.INSERT_ANSWER;
                }
            }
            if (insertOptions.flag == InsertOptions.INSERT_ANSWER)
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
                    Logger.Info(userInsertName + "введенный ответ №" + insertOptions.countQ.ToString()
                        + ": " + message.Text);
                    if (insertOptions.countQ < insertOptions.count)
                    {
                        insertOptions.flag = InsertOptions.INSERT_ANSWER;
                        await SendInsertMsg(message.Chat.Id, "Введите " + (insertOptions.countQ + 1).ToString() + "-й вариант ответа", insertOptions, insertUser, Bot);
                        insertOptions.countQ++;
                        return;
                    }
                    else
                    {
                        if (insertOptions.count != 1)
                        {
                            insertOptions.flag = InsertOptions.INSERT_RIGHT;

                            string[] temp = new string[insertOptions.count];
                            for (int i = 1; i <= insertOptions.count; i++)
                            {
                                temp[i - 1] = i.ToString();
                            }
                            await SendMsgKeyboardInsert(message.Chat.Id, "Выберите номер верного варианта ответа",
                                temp, insertOptions, Bot, insertUser);
                            return;
                        }
                        else
                        {
                            insertOptions.flag = InsertOptions.INSERT_TYPE_ANSWER;
                            insertOptions.newQuestion.CorrectAnswer = insertOptions.newQuestion.PossibleAnswer_1;
                            insertOptions.newQuestion.PossibleAnswer_1 = "";
                            insertOptions.newQuestion.TypeAnswer = 0;
                        }
                    }
                }
            }
            if (insertOptions.flag == InsertOptions.INSERT_RIGHT)
            {
                bool res = int.TryParse(insertOptions.btnText, out int n);
                if (!res || (n > insertOptions.count) || (n < 1))
                {
                    insertOptions.flag = InsertOptions.INSERT_RIGHT;
                    return;
                }
                else
                {
                    insertOptions.newQuestion.CorrectAnswer = n.ToString();
                    Logger.Info(userInsertName + "верный вариант ответа: " + message.Text);
                    insertOptions.flag = InsertOptions.INSERT_ANSWER_METHOD;
                }
            }
            if (insertOptions.flag == InsertOptions.INSERT_ANSWER_METHOD && insertOptions.count > 1)
            {
                insertOptions.flag = InsertOptions.INSERT_TYPE_ANSWER;
                await SendMsgKeyboardInsert(message.Chat.Id, "Выберите способ ответа",
                    new string[] { "Реплай", "Кнопки" }, insertOptions, Bot, insertUser);
                return;
            }
            else
            if (insertOptions.flag == InsertOptions.INSERT_TYPE_ANSWER && insertOptions.count == 1)
            {
                insertOptions.newQuestion.TypeAnswer = 0;
                Logger.Info(userInsertName + "тип ответа: реплай");
                insertOptions.flag = InsertOptions.INSERT_CORRECT;
            }
            if (insertOptions.flag == InsertOptions.INSERT_TYPE_ANSWER)
            {
                bool res = int.TryParse(insertOptions.btnText, out int n);
                if (res)
                {
                    insertOptions.newQuestion.TypeAnswer = n - 1;
                    Logger.Info(userInsertName + "тип ответа: " + message.Text);
                    insertOptions.flag = InsertOptions.INSERT_CORRECT;
                }
                else
                if (!res || (n != 2 || n != 1))
                {
                    insertOptions.flag = InsertOptions.INSERT_TYPE_ANSWER;
                    await SendMsgKeyboardInsert(message.Chat.Id, "Выберите способ ответа",
                        new string[] { "Реплай", "Кнопки" }, insertOptions, Bot, insertUser);
                    return;
                }
            }
            if (insertOptions.flag == InsertOptions.INSERT_CORRECT)
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
                Logger.Info(userInsertName + txtQuest);
                insertOptions.flag = InsertOptions.INSERT_END;
                await SendMsgKeyboardInsert(message.Chat.Id, txtQuest,
                    new string[] { "Отмена", "Ок" }, insertOptions, Bot, insertUser);
                return;
            }
            if (insertOptions.flag == InsertOptions.INSERT_END && insertOptions.btnText == "2")
            {
                Logger.Info(userInsertName + "insert newQuestion in base");
                if (config.InsertIssues(insertOptions.newQuestion, message.From.Id, Program.GetUserName(message)))
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ваш вопрос добавлен\n\nДобавить новый вопрос /insert");
                    Logger.Info(userInsertName + " insert true");
                }
                else
                {
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Ошибка при добавлении вопроса");
                    Logger.Info(userInsertName + " insert false");
                }
                insertOptions.insert = false;
                insertUser.Remove(message.Chat.Id);
            }
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Редактирования пока нет, вводите все заново, раз так!\n\nДобавить новый вопрос /insert");
                Logger.Info(userInsertName + "incorrect");
                insertOptions.insert = false;
                insertUser.Remove(message.Chat.Id);
            }
        }

        static async Task SendInsertMsg(long chatId, string text, InsertOptions insertOptions,
            Hashtable insertUser, TelegramBotClient Bot)
        {
            await Bot.SendTextMessageAsync(chatId, text);
            insertUser[chatId] = insertOptions;
        }
    }
}
