using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotGame
{
    public class DeleteOptions
    {
        public int begin = 0;
        public int count = 10;
        public Hashtable issuesDelete = new Hashtable();
        public int maxId;
        public Telegram.Bot.Types.Message msg;
    }
}