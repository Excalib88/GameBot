using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BotGame
{
    public static class Statistics
    {
        public static User GetStatistics(List<MessageOUT> messageOUT, ConfigSQL config)
        {
            List<MessageOUT> newMsg = messageOUT.FindAll(q => !String.IsNullOrEmpty(q.userWin.Name));

            int max = messageOUT.Max(a => a.AttemptsAnswers);            

            string idQuestionAttempts = "";
            if (max > 1)
            {
                List<MessageOUT> resultAttemptsAnswers = newMsg.FindAll(a => a.AttemptsAnswers == max);
                foreach (MessageOUT m in resultAttemptsAnswers)
                {
                    Program.SendMsg(m.ChatId, "Вопрос с наибольшим количеством попыток:\n" + m.MessageText);
                    idQuestionAttempts += m.QuestionId.ToString() + ", ";
                }
            }

            TimeSpan time = newMsg.Max(a => a.Time);
            List<MessageOUT> resultTime = newMsg.FindAll(a => a.Time == time);

            string idQuestionTime = "";
            foreach (MessageOUT m in resultTime)
            {
                Program.SendMsg(m.ChatId, "Вопрос с самым длительным временем ответа:\n" + m.MessageText);
                idQuestionTime += m.QuestionId + ", ";
            }
            
            Program.SendMsg(m.ChatId, "Вопрос с самым длительным временем ответа:\n" + m.MessageText);

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
            foreach (int i in id)
            {                
                number = newMsg.Count(p => p.userWin.Name == user[i].ToString());
                if (number > r)
                {
                    userQ = new User
                    {
                        Id = i,
                        Name = user[i].ToString()
                    };
                    r = number;
                }
            }
            config.SaveStatistics(userQ, idQuestionAttempts, idQuestionTime);
            return userQ;
        }
    }
}