namespace BotGame
{
    public class IssuesClass
    {
        public const int TYPE_ANSWER_REPLY = 0;
        public const int TYPE_ANSWER_BUTTON = 1;

        public int Id;
        public string QuestionText;
        public string CorrectAnswer;
        public string PossibleAnswer_1;
        public string PossibleAnswer_2;
        public string PossibleAnswer_3;
        public string PossibleAnswer_4;
        public string PossibleAnswer_5;
        public int Complexity;
        public string Category;
        public int TypeAnswer;
    }
}