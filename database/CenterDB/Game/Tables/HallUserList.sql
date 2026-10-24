CREATE TABLE [Game].[HallUserList] (
    [UserID]     INT      NOT NULL,
    [ServerID]   SMALLINT NOT NULL,
    [HDType]     TINYINT  NOT NULL,
    [CreateTime] DATETIME DEFAULT (getdate()) NULL
);

