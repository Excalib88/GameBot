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

        public ConfigSQL()
        {
            issues = new Hashtable();
            connection = new SQLiteConnection(string.Format("Data Source={0};", PATH_BASE));
                        
            try
            {
                connection.Open();

                SQLiteCommand commandKey = new SQLiteCommand("select key from 'settings';", connection);
                SQLiteDataReader readerKey = commandKey.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {
                    KEY = record["key"].ToString();
                }

                SQLiteCommand command = new SQLiteCommand("select id, question_text," +
                    "correct_answer, possible_answer_1, possible_answer_2, possible_answer_3, " +
                    "complexity, category from 'issues';", connection);
                SQLiteDataReader reader = command.ExecuteReader();
                foreach (DbDataRecord record in reader)
                {
                    if (!issues.ContainsKey(Convert.ToInt32(record["id"])))
                        issues.Add(Convert.ToInt32(record["id"]), 
                            new IssuesClass
                            {
                                QuestionText = record["question_text"].ToString(),                                
                                CorrectAnswer = record["correct_answer"].ToString(),
                                PossibleAnswer_1 = record["possible_answer_1"].ToString(),
                                PossibleAnswer_2 = record["possible_answer_2"].ToString(),
                                PossibleAnswer_3 = record["possible_answer_3"].ToString(),
                                Complexity = Convert.ToInt32(record["complexity"]),
                                Category = record["category"].ToString()
                            });
                }
                connection.Close();                
            }
            catch
            {
                connection.Close();
            }
        }
    }
}