using System;
using System.Collections;
using System.Data.Common;
using System.Data.SQLite;

namespace BotGame
{
    public class ConfigSQL
    {
        SQLiteConnection connection;
        private const string PATH_BASE = "BotGame.db";
        public Hashtable issues;

        private string key;
        private string IdBot;// = "460362250";

        public string KEY
        {
            get
            {
                return key;
            }
            private set
            {
                key = value;
            }
        }

        public string IDBOT
        {
            get
            {
                return IdBot;
            }
            private set
            {
                IdBot = value;
            }
        }

        public ConfigSQL()
        {
            issues = new Hashtable();
            connection = new SQLiteConnection(string.Format("Data Source={0};", PATH_BASE));

            try
            {
                connection.Open();

                SQLiteCommand commandKey = new SQLiteCommand("select key, value from 'settings';", connection);
                SQLiteDataReader readerKey = commandKey.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {
                    if (record["key"].ToString() == "token")
                    {
                        KEY = record["value"].ToString();
                    }
                    if (record["key"].ToString() == "id_bot")
                    {
                        IDBOT = record["value"].ToString();
                    }
                }
                connection.Close();
                Logger.Info("select settings from base");
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
        }

        public void SelectQuestion()
        {
            try
            {
                connection.Open();

                SQLiteCommand command = new SQLiteCommand("select id, question_text," +
                    "correct_answer, possible_answer_1, possible_answer_2, possible_answer_3, " +
                    "complexity, category, type_answer from 'issues';", connection);
                SQLiteDataReader reader = command.ExecuteReader();
                foreach (DbDataRecord record in reader)
                {
                    if (!issues.ContainsKey(Convert.ToInt32(record["id"])))
                    {
                        int complexity = String.IsNullOrEmpty(record["complexity"].ToString()) ? 0 : Convert.ToInt32(record["complexity"]);
                        int typeAnswer = String.IsNullOrEmpty(record["type_answer"].ToString()) ? 0 : Convert.ToInt32(record["type_answer"]);
                        issues.Add(Convert.ToInt32(record["id"]),
                            new IssuesClass
                            {
                                QuestionText = record["question_text"].ToString(),
                                CorrectAnswer = record["correct_answer"].ToString(),
                                PossibleAnswer_1 = record["possible_answer_1"].ToString(),
                                PossibleAnswer_2 = record["possible_answer_2"].ToString(),
                                PossibleAnswer_3 = record["possible_answer_3"].ToString(),
                                Complexity = complexity,
                                Category = record["category"].ToString(),
                                TypeAnswer = typeAnswer
                            });
                    }
                }
                connection.Close();
                Logger.Success("select issues from base");
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
        }
    }
}