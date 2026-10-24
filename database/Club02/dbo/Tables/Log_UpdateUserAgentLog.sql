CREATE TABLE [dbo].[Log_UpdateUserAgentLog] (
    [LogID]     BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]   DATETIME NOT NULL,
    [UserID]    INT      NULL,
    [SubUserID] INT      NULL,
    [Stage1]    TINYINT  NULL,
    [Stage2]    TINYINT  NULL,
    [Stage3]    TINYINT  NULL,
    [BF_Stage1] TINYINT  NULL,
    [BF_Stage2] TINYINT  NULL,
    [BF_Stage3] TINYINT  NULL,
    [Status]    TINYINT  NULL,
    [BF_Status] TINYINT  NULL,
    CONSTRAINT [PK_Log_UpdateUserAgentLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

