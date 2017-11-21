CREATE TABLE statistics (
    id                    INTEGER PRIMARY KEY AUTOINCREMENT
                                  NOT NULL,
    date_game             VARCHAR,
    user_win_id           INTEGER,
    user_win_name         VARCHAR,
    username_win_telegram INTEGER,
    id_question_attempt   VARCHAR,
    id_question_time      VARCHAR,
    chat_id               VARCHAR,
    chat_name             VARCHAR
);
