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
        public List<int> questionNumber = new List<int>();
        public int num;
        public bool end = false;
        public bool game = false;
        public Hashtable issues;
        public int complexity;

        public string ComplexityText
        {
            get
            {
                string res = default;
                if (complexity == 0)
                    res = "easy";
                else if (complexity == 1)
                    res = "medium";
                else if (complexity == 2)
                    res = "hard";
                else if (complexity == 3)
                    res = "Slavik";

                return res;
            }
        }
    }
}