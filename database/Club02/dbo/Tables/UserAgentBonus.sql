CREATE TABLE [dbo].[UserAgentBonus] (
    [ID]         BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]    DATETIME NOT NULL,
    [LogHour]    INT      NOT NULL,
    [UserID]     INT      NOT NULL,
    [UserLevel]  TINYINT  NOT NULL,
    [Rate]       INT      NOT NULL,
    [Amount]     BIGINT   NOT NULL,
    [RelationID] BIGINT   NOT NULL,
    [FromUserID] INT      NULL,
    CONSTRAINT [PK_UserAgentBonus] PRIMARY KEY CLUSTERED ([ID] ASC)
);

