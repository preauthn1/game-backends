CREATE TABLE [dbo].[NetProfitAgent_Pool] (
    [UserID]         INT      NOT NULL,
    [PoolAmount]     BIGINT   NOT NULL,
    [WithDrawAmount] BIGINT   NOT NULL,
    [Rate]           TINYINT  NOT NULL,
    [CreateTime]     DATETIME NOT NULL,
    [UpdateTime]     DATETIME NULL,
    CONSTRAINT [PK_NetProfitAgent_Pool] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

