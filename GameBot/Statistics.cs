using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BotGame
{
    public static class Statistics
    {
        private static List<MessageOUT> messageOUT;
        private static List<MessageOUT> newMsg;
        private static string idQuestionAttempts;
        private static string idQuestionTime;
        private static List<User> userQ;
        private static List<User> allUser;


        private static string SelectMaxAttempts()
        {
            string textMsg = "";
            int max = messageOUT.Max(a => a.AttemptsAnswers);
                        
            if (max > 1)
            {
                textMsg += "Вопросы с наибольшим количеством попыток:\n";
                List<MessageOUT> resultAttemptsAnswers = newMsg.FindAll(a => a.AttemptsAnswers == max);
                foreach (MessageOUT m in resultAttemptsAnswers)
                {
                    textMsg += m.MessageText + "\n";
                    idQuestionAttempts += m.QuestionId.ToString() + ", ";
                }
            }
            return textMsg;
        }

        private static string SelectMaxTime()
        {
            string textMsg = "";
            TimeSpan time = newMsg.Max(a => a.Time);
            List<MessageOUT> resultTime = newMsg.FindAll(a => a.Time == time);
            
            textMsg += "\nВопросы с самым длительным временем ответа:\n";
            foreach (MessageOUT m in resultTime)
            {
                textMsg += m.MessageText + "\n";
                idQuestionTime += m.QuestionId + ", ";
            }
            return textMsg;
        }

        private static void SelectUserWin()
        {
            Hashtable user = new Hashtable();
            foreach (MessageOUT m in newMsg)
            {
                if (!String.IsNullOrEmpty(m.userWin.Name))
                    if (!user.ContainsKey(m.userWin.Id))
                        user.Add(m.userWin.Id, m.userWin.Name);
            }

            var id = user.Keys;

            int number = -1;
            int r = -1;           
            allUser = new List<User>();
            userQ = new List<User>();
            foreach (int i in id)
            {
                number = newMsg.Count(p => p.userWin.Name == user[i].ToString());
                if (number > r)
                {
                    userQ.Add(new User
                    {
                        Id = i,
                        Name = user[i].ToString(),
                    });
                    r = number;
                }
                allUser.Add(
                    new User
                    {
                        Id = i,
                        Name = user[i].ToString(),
                        countCorrectAnswer = number
                    });
            }
        }

        public static string GetStatistics(List<MessageOUT> messageOUT1, ConfigSQL config, long ChatId, string ChatName)
        {
            messageOUT = messageOUT1;
            string textMsg = "Конец игры\n\n";
            newMsg = messageOUT.FindAll(q => !String.IsNullOrEmpty(q.userWin.Name));

            textMsg += SelectMaxAttempts() + SelectMaxTime();            
            
            Program.SendMsg(newMsg[0].ChatId, textMsg);

            SelectUserWin();

            string win = "Победители в игре:\n";
            foreach (User u in userQ)
            {
                win += u.Name + "\n";
                config.SaveStatistics(u, idQuestionAttempts, idQuestionTime, ChatId, ChatName);
            }
            win += "\n" + "Статистика игры:\n\n";

            foreach (User u in allUser)
            {
                win += u.Name + ": количество правильных ответов " + u.countCorrectAnswer.ToString() + "\n";
            }            
            
            return win;
        }
    }
}