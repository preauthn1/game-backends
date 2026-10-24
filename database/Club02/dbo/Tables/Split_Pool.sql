CREATE TABLE [dbo].[Split_Pool] (
    [UserID]             INT      NOT NULL,
    [PoolAmount]         BIGINT   NOT NULL,
    [WithDrawAmount]     BIGINT   NOT NULL,
    [SuccessSplitAmount] BIGINT   NOT NULL,
    [CreateTime]         DATETIME NOT NULL,
    [UpdateTime]         DATETIME NULL,
    CONSTRAINT [PK_Split_Pool] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

