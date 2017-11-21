using System.Collections;
using System.Collections.Generic;

namespace BotGame
{
    public class Game
    {
        public MessageOUT msgOUTobject;
        public MessageIN msgINobject;
        public IssuesClass issuesObject;
        public List<MessageOUT> messageOUTobject;
        public List<MessageIN> messageINobject;
        public List<int> questionNumber;
        public int num;
        public bool end;
        public bool game;
        public Hashtable issues;
    }
}