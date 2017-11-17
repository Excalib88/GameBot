using System;

namespace BotGame
{
    public class MessageIN
    {
        public long ChatId;
        public int MessageId;
        public string UserId;
        public string UserName;
        //public string UserLastName;
        public string UserUsername;
        public string MessageText;
        public DateTime MmessageDate;

        public int ReplayToMessageId;
        public string ReplayToMessageText;
        public int ReplayToUserId;
    }
}