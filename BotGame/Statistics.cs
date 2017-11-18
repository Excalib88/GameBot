using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BotGame
{
    public static class Statistics
    {
        public static User GetStatistics(List<MessageOUT> messageOUT)
        {
            List<MessageOUT> newMsg = messageOUT.FindAll(q => !String.IsNullOrEmpty(q.userWin.Name));

            int max = messageOUT.Max(a => a.AttemptsAnswers);
            List<MessageOUT> resultAttemptsAnswers = newMsg.FindAll(a => a.AttemptsAnswers == max);

            TimeSpan time = newMsg.Max(a => a.Time);
            List<MessageOUT> resultTime = newMsg.FindAll(a => a.Time == time);

            // int numberUnvaccinated = pets.Count(p => p.Vaccinated == false);
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
            // int resultUser = messageOUT.Count(a => a.UserIdWin);

            // config.SaveStatistics();
            return userQ;
        }
    }
}