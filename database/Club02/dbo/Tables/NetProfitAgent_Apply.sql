CREATE TABLE [dbo].[NetProfitAgent_Apply] (
    [ID]           INT          IDENTITY (1, 1) NOT NULL,
    [UserID]       INT          NOT NULL,
    [TopUserID]    INT          NULL,
    [Status]       INT          NOT NULL,
    [CreateTime]   DATETIME     NOT NULL,
    [UpdateTime]   DATETIME     NULL,
    [Updater]      VARCHAR (30) NULL,
    [AgentUpdater] INT          NULL,
    CONSTRAINT [PK_NetProfitAgent_Apply] PRIMARY KEY CLUSTERED ([ID] ASC)
);

