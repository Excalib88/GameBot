create database BotGame;

CREATE TABLE BotGame.statistics (
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

CREATE TABLE BotGame.settings (
    [key] VARCHAR,
    value VARCHAR
);

CREATE TABLE BotGame.issues (
    id                INTEGER PRIMARY KEY AUTOINCREMENT
                              NOT NULL,
    question_text     VARCHAR NOT NULL,
    correct_answer    VARCHAR NOT NULL,
    possible_answer_1 VARCHAR,
    possible_answer_2 VARCHAR,
    possible_answer_3 VARCHAR,
    possible_answer_4 VARCHAR,
    possible_answer_5 VARCHAR,
    complexity        INTEGER,
    category          VARCHAR,
    type_answer       INTEGER,
    desc              VARCHAR
);