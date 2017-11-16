namespace BotGame
{
    public class Message
    {
        public long ChatId;
        public long MessageId;
        public long UserId;
        public string UserFirstName;
        public string UserLastName;
        public string UserUsername;
        public string MessageText;
        public string MmessageDate;

        public long ReplayToMessageId;
        public string ReplayToMessageText;
        public long ReplayToUserId;
    }
}