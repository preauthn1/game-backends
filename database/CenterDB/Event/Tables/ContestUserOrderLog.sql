CREATE TABLE [Event].[ContestUserOrderLog] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [YYYYMMDD]   INT      NOT NULL,
    [UserID]     INT      NOT NULL,
    [Amount]     BIGINT   NOT NULL,
    [CreateTime] DATETIME CONSTRAINT [DF_ContestUserOrderLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ContestUserOrderLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

