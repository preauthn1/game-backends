CREATE TABLE [dbo].[NetProfitAgent_WithDrawLog] (
    [ID]                 BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserID]             INT           NOT NULL,
    [WithDrawAmount]     BIGINT        NOT NULL,
    [WithDrawRate]       INT           NULL,
    [PoolAmount]         BIGINT        NOT NULL,
    [PoolWithDrawAmount] BIGINT        NOT NULL,
    [PoolLastUpdateTime] DATETIME      NOT NULL,
    [Status]             TINYINT       NOT NULL,
    [CreateTime]         DATETIME      NOT NULL,
    [Updatetime]         DATETIME      NULL,
    [UpdateUser]         VARCHAR (50)  NULL,
    [Memo]               VARCHAR (100) NULL,
    CONSTRAINT [PK_NetProfitAgent_WithDrawLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

