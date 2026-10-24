CREATE TABLE [dbo].[Split_WithDrawLog] (
    [ID]                 BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserID]             INT           NOT NULL,
    [WithDrawAmount]     BIGINT        NOT NULL,
    [WithDrawRate]       TINYINT       NULL,
    [PoolAmount]         BIGINT        NOT NULL,
    [PoolWithDrawAmount] BIGINT        NOT NULL,
    [PoolLastUpdateTime] DATETIME      NULL,
    [Status]             TINYINT       NULL,
    [CreateTime]         DATETIME      NOT NULL,
    [Updatetime]         DATETIME      NULL,
    [UpdateUser]         VARCHAR (50)  NULL,
    [Memo]               VARCHAR (100) NULL,
    CONSTRAINT [PK_Split_WithDrawLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

