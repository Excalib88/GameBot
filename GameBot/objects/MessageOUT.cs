﻿using System;

namespace BotGame
{
    public class MessageOUT
    {
        public long ChatId;
        public int MessageId;
        public string MessageText;
        public int QuestionId;
        public DateTime MmessageDate;
        public DateTime AnswerDate;
        //public DateTime Time;
        //public string UserNameWin;
        //public int UserIdWin;
        public int AttemptsAnswers = 0;

        public User userWin;

        public TimeSpan Time
        {
            //TimeSpan ts = newDate - oldDate;
            get
            {
                return AnswerDate - MmessageDate;
            }            
        }
    }
}
