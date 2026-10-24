CREATE TABLE [Stat].[AgentTransferSourceLog] (
    [Id]       INT      IDENTITY (1, 1) NOT NULL,
    [AgentID]  INT      NOT NULL,
    [SourceID] INT      NOT NULL,
    [Amount]   BIGINT   NOT NULL,
    [StatTime] DATETIME NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_AgentTransferSourceLog_StatTime_INCLUDE]
    ON [Stat].[AgentTransferSourceLog]([StatTime] ASC);

