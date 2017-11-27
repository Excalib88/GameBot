namespace BotGame
{
    public class InsertOptions
    {
        public const int INSERT_FALSE = -1;
        public const int INSERT_START = 0;
        public const int INSERT_COUNT_ANSWER = 1;
        public const int INSERT_CORRECT_COUNT_ANSWER = 2;
        public const int INSERT_ANSWER = 3;
        public const int INSERT_RIGHT = 4;
        public const int INSERT_ANSWER_METHOD = 5;
        public const int INSERT_TYPE_ANSWER = 6;
        public const int INSERT_CORRECT = 7;
        public const int INSERT_END = 8;

        public string btnText;

        public bool insert;
        public int flag;// = -1;
        public int count;// = -1;
        public int countQ;// = 0;
        public IssuesClass newQuestion;
    }
}
