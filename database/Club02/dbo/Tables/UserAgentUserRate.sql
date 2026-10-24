CREATE TABLE [dbo].[UserAgentUserRate] (
    [UserID]    INT     NOT NULL,
    [UserLevel] TINYINT NOT NULL,
    [Stage1]    INT     NOT NULL,
    [Stage2]    INT     NOT NULL,
    [Stage3]    INT     NOT NULL,
    CONSTRAINT [PK_UserAgentUserRate_1] PRIMARY KEY CLUSTERED ([UserID] ASC, [UserLevel] ASC)
);

