using System;

namespace BotGame
{
    public class MessageOUT
    {
        public long ChatId;
        public int MessageId;
        public string MessageText;
        public DateTime MmessageDate;
        public DateTime AnswerDate;
        public string UserWin;
        public int AttemptsAnswers = 0;
    }
}
