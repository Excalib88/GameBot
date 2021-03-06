﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;

namespace BotGame
{
    public class ConfigSQL
    {
        private SQLiteConnection connection;

        private const string PATH_BASE = "BotGame.db";
        private const string NAME_DELETION_DELAY = "deletion_delay";
        private const string NAME_TOKEN = "token";
        private const string NAME_ID_BOT = "id_bot";
        private const string NAME_ADMIN = "admin";
        private const string RULE_INSERT = "rule_insert";
        private const string NAME_BOT = "name_bot";

        private Hashtable settings;

        #region getset

        public string NameBot
        {
            get
            {
                return settings[NAME_BOT].ToString();
            }
            private set
            {
                settings[NAME_BOT] = value;
            }
        }

        public int RuleInsert
        {
            get
            {
                return Convert.ToInt32(settings[RULE_INSERT]);
            }
            private set
            {
                settings[RULE_INSERT] = value;
            }
        }

        public int DeletionDelay
        {
            get
            {
                return Convert.ToInt32(settings[NAME_DELETION_DELAY]);
            }
            private set
            {
                settings[NAME_DELETION_DELAY] = value;
            }
        }

        public string TOKEN
        {
            get
            {
                return settings[NAME_TOKEN].ToString();
            }
            private set
            {
                settings[NAME_TOKEN] = value;
            }
        }

        public int IDBOT
        {
            get
            {
                return Convert.ToInt32(settings[NAME_ID_BOT]);
            }
            private set
            {
                settings[NAME_ID_BOT] = value;
            }
        }

        public List<string> ADMIN
        {
            get
            {
                List<string> tolist = settings[NAME_ADMIN].ToString().Split(';').ToList<string>();
                return tolist;
            }
            private set
            {
                settings[NAME_ADMIN] = value;
            }
        }
        #endregion getset

        public ConfigSQL()
        {
            settings = new Hashtable();            
            connection = new SQLiteConnection(string.Format("Data Source={0};", PATH_BASE));
            Logger.Info("select settings from base");
            try
            {
                connection.Open();
                SQLiteCommand commandKey = new SQLiteCommand("select key, value from 'settings';", connection);
                Logger.Info(commandKey.CommandText);
                SQLiteDataReader readerKey = commandKey.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {
                    if (!settings.ContainsKey(record["key"].ToString()))
                        settings.Add(record["key"].ToString(), record["value"]);
                }
                readerKey.Close();
                connection.Close();
                foreach (string name in new string[] { NAME_DELETION_DELAY, NAME_TOKEN, NAME_ID_BOT, NAME_ADMIN, RULE_INSERT, NAME_BOT })
                {
                    if (!settings.ContainsKey(name))
                    {                        
                        Logger.Warn("Не найден настроечный параметр " + name);
                    }
                }                
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
        }

        public Hashtable SelectQuestion(int complexityIn = default)
        {
            Hashtable issues = new Hashtable();
            try
            {
                connection.Open();
                SQLiteCommand command = new SQLiteCommand("select id, question_text," +
                    "correct_answer, possible_answer_1, possible_answer_2, possible_answer_3, " +
                    "possible_answer_4, possible_answer_5," +
                    "complexity, category, type_answer from 'issues' " +
                    "where complexity = @complexity;", connection);
                command.Parameters.AddWithValue("@complexity", complexityIn);
                Logger.Info(command.CommandText);
                SQLiteDataReader reader = command.ExecuteReader();
                foreach (DbDataRecord record in reader)
                {
                    if (!issues.ContainsKey(Convert.ToInt32(record["id"])))
                    {
                        int id = Convert.ToInt32(record["id"]);
                        int complexity = String.IsNullOrEmpty(record["complexity"].ToString()) ? 0 : Convert.ToInt32(record["complexity"]);
                        int typeAnswer = String.IsNullOrEmpty(record["type_answer"].ToString()) ? 0 : Convert.ToInt32(record["type_answer"]);
                        issues.Add(Convert.ToInt32(record["id"]),
                            new IssuesClass
                            {
                                Id = id,
                                QuestionText = record["question_text"].ToString().Replace("@BR", Environment.NewLine),
                                CorrectAnswer = record["correct_answer"].ToString(),
                                PossibleAnswer_1 = record["possible_answer_1"].ToString(),
                                PossibleAnswer_2 = record["possible_answer_2"].ToString(),
                                PossibleAnswer_3 = record["possible_answer_3"].ToString(),
                                PossibleAnswer_4 = record["possible_answer_4"].ToString(),
                                PossibleAnswer_5 = record["possible_answer_5"].ToString(),
                                Complexity = complexity,
                                Category = record["category"].ToString(),
                                TypeAnswer = typeAnswer
                            });
                    }
                }
                reader.Close();
                connection.Close();
                Logger.Success("select issues from base");
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
            return issues;
        }

        public bool InsertIssues(IssuesClass issues, int userId, string userName)
        {
            try
            {
                connection.Open();
                SQLiteCommand command1 = new SQLiteCommand("insert into issues (question_text, correct_answer, " +
                    "possible_answer_1, possible_answer_2, possible_answer_3, possible_answer_4, possible_answer_5, " +
                    "type_answer, complexity)" +
                    "select @question_text, '" + issues.CorrectAnswer.ToString() + "', " +
                    "@possible_answer_1, @possible_answer_2, @possible_answer_3, @possible_answer_4, @possible_answer_5, " +
                     "@type_answer, @complexity; ", connection);
                Logger.Info(command1.CommandText);
                command1.Parameters.AddWithValue("@question_text", issues.QuestionText.Replace("\n","@BR"));
                command1.Parameters.AddWithValue("@possible_answer_1", issues.PossibleAnswer_1);
                command1.Parameters.AddWithValue("@possible_answer_2", issues.PossibleAnswer_2);
                command1.Parameters.AddWithValue("@possible_answer_3", issues.PossibleAnswer_3);
                command1.Parameters.AddWithValue("@possible_answer_4", issues.PossibleAnswer_4);
                command1.Parameters.AddWithValue("@possible_answer_5", issues.PossibleAnswer_5);
                command1.Parameters.AddWithValue("@type_answer", issues.TypeAnswer);
                command1.Parameters.AddWithValue("@complexity", issues.Complexity);
                command1.ExecuteNonQuery();
                connection.Close();
                Logger.Success("save issues in base");
                return true;
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
                return false;
            }
        }

        public string SelectWin(long chatId)
        {
            string result = "";
            try
            {
                connection.Open();
                SQLiteCommand command2 = new SQLiteCommand("select count (user_win_name) as count, " +
                    "user_win_name as name from statistics where chat_id = @chat_id " +
                    "group by user_win_name " +
                    "order by count (user_win_name) desc", connection);
                Logger.Info(command2.CommandText);
                command2.Parameters.AddWithValue("@chat_id", chatId);
                SQLiteDataReader readerKey = command2.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {
                    result += record["name"].ToString() + ": " + record["count"].ToString() + " win\n";
                }
                readerKey.Close();
                connection.Close();
                Logger.Success("select count win from base");
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
            return result;
        }

        public string CountIssue()
        {
            string result = "";
            try
            {
                connection.Open();
                SQLiteCommand command2 = new SQLiteCommand("select count (id) as count from issues;", connection);
                Logger.Info(command2.CommandText);

                SQLiteDataReader readerKey = command2.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {
                    result = record["count"].ToString();
                }
                readerKey.Close();
                connection.Close();
                Logger.Success("select count issues from base");                
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);                
            }
            return result;
        }

        public bool DeleteIssue(int id)
        {
            try
            {
                connection.Open();
                SQLiteCommand command1 = new SQLiteCommand("delete from issues " +
                    "where id = @id", connection);
                Logger.Info(command1.CommandText);
                command1.Parameters.AddWithValue("@id", id);                                
                command1.ExecuteNonQuery();
                connection.Close();
                Logger.Success("delete issues in base");
                return true;
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
                return false;
            }
        }

        public string SelectCountGame(long ChatId)
        {
            string result = "0";
            try
            {
                connection.Open();
                SQLiteCommand command1 = new SQLiteCommand("select count (chat_id) as count from 'statistics' " +
                    "where chat_id = @ChatId;", connection);
                Logger.Info(command1.CommandText);
                command1.Parameters.AddWithValue("@ChatId", ChatId.ToString());
                SQLiteDataReader readerKey = command1.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {
                    result = record["count"].ToString();                    
                }
                readerKey.Close();
                connection.Close();
                Logger.Success("select count game in base: " + result);
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
            return result;
        }

        public DeleteOptions SelectForDelete()
        {
            DeleteOptions delete = new DeleteOptions();
            try
            {
                connection.Open();
                SQLiteCommand command2 = new SQLiteCommand("select max (id) as maxId from issues;", connection);
                Logger.Info(command2.CommandText);
                SQLiteDataReader reader = command2.ExecuteReader();
                foreach (DbDataRecord record in reader)
                {
                    delete.maxId = Convert.ToInt32(record["maxId"]);                    
                }
                SQLiteCommand command1 = new SQLiteCommand("select id, question_text from issues;", connection);
                Logger.Info(command1.CommandText);
                SQLiteDataReader readerKey = command1.ExecuteReader();
                foreach (DbDataRecord record in readerKey)
                {                    
                    if (!delete.issuesDelete.ContainsKey(record["id"].ToString()))
                        delete.issuesDelete.Add(Convert.ToInt32(record["id"]), record["question_text"].ToString().Replace("@BR","\n"));
                }
                reader.Close();
                connection.Close();
                Logger.Success("select for delete");
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
            return delete;
        }

        public void SaveStatistics(User user, string questionAttempt, string questionTime, long ChatId, string ChatName)
        {
            string username = !String.IsNullOrEmpty(user.Username) ? user.Username : "";
            try
            {
                connection.Open();
                SQLiteCommand command1 = new SQLiteCommand("insert into 'statistics' (date_game, user_win_id, user_win_name, " +
                    "username_win_telegram, id_question_attempt, id_question_time, " +
                    "chat_id, chat_name) " +
                    "select @DateTime, @userId, @user_Name, @username"
                    + ", @question_attempt, @question_time, @chat_id, @chat_name;", connection);
                Logger.Info(command1.CommandText);
                command1.Parameters.AddWithValue("@DateTime", DateTime.Now.ToString());
                command1.Parameters.AddWithValue("@userId", user.Id);
                command1.Parameters.AddWithValue("@user_Name", user.Name);
                command1.Parameters.AddWithValue("@username", username);
                command1.Parameters.AddWithValue("@question_attempt", questionAttempt);
                command1.Parameters.AddWithValue("@question_time", questionTime);
                command1.Parameters.AddWithValue("@chat_id", ChatId.ToString());
                command1.Parameters.AddWithValue("@chat_name", ChatName);
                command1.ExecuteNonQuery();
                connection.Close();
                Logger.Success("save statistics in base");
            }
            catch (Exception e)
            {
                connection.Close();
                Logger.Error(e.Message);
            }
        }
    }
}