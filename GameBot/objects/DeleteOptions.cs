using System.Collections;

namespace BotGame
{
    public class DeleteOptions
    {
        public bool delete;
        public int begin = 0;
        public int count = 10;
        public Hashtable issuesDelete = new Hashtable();
        public int maxId;
        public Telegram.Bot.Types.Message msg;
    }
}