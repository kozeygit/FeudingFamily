CREATE TABLE Answers
(
    Id         INTEGER PRIMARY KEY AUTOINCREMENT,
    Content    TEXT NOT NULL,
    Points     INTEGER,
    Ranking    INTEGER,
    QuestionId INTEGER,
    FOREIGN KEY (QuestionId) REFERENCES Questions (Id)
);
