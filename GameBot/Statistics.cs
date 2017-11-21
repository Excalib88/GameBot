using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BotGame
{
    public static class Statistics
    {
        public static string GetStatistics(List<MessageOUT> messageOUT, ConfigSQL config, long ChatId, string ChatName)
        {
            string textMsg = "Конец игры\n\n";
            List<MessageOUT> newMsg = messageOUT.FindAll(q => !String.IsNullOrEmpty(q.userWin.Name));

            int max = messageOUT.Max(a => a.AttemptsAnswers);            

            string idQuestionAttempts = "";
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

            TimeSpan time = newMsg.Max(a => a.Time);
            List<MessageOUT> resultTime = newMsg.FindAll(a => a.Time == time);

            string idQuestionTime = "";
            textMsg += "\nВопросы с самым длительным временем ответа:\n";
            foreach (MessageOUT m in resultTime)
            {
                textMsg += m.MessageText + "\n";
                idQuestionTime += m.QuestionId + ", ";
            }
            
            Program.SendMsg(newMsg[0].ChatId, textMsg);

            Hashtable user = new Hashtable();
            foreach (MessageOUT m in newMsg)
            {
                if (!String.IsNullOrEmpty(m.userWin.Name))
                    if (!user.ContainsKey(m.userWin.Id))
                        user.Add(m.userWin.Id, m.userWin. Name);
            }

            var id = user.Keys;

            int number = -1;
            int r = -1;
            User userQ = null;
            List<User> allUser = new List<User>();
            foreach (int i in id)
            {                
                number = newMsg.Count(p => p.userWin.Name == user[i].ToString());
                if (number > r)
                {
                    userQ = new User
                    {
                        Id = i,
                        Name = user[i].ToString(),
                    };
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

            string win = "Победитель в игре - " + userQ.Name + "\n\n" +
                "Статистика игры:\n\n";

            foreach (User u in allUser)
            {
                win += u.Name + ": количество правильных ответов " + u.countCorrectAnswer.ToString() + "\n";
            }

            config.SaveStatistics(userQ, idQuestionAttempts, idQuestionTime, ChatId, ChatName);
            
            return win;
        }
    }
}