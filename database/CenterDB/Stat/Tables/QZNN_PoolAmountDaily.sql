CREATE TABLE [Stat].[QZNN_PoolAmountDaily] (
    [LogDate] DATETIME NOT NULL,
    [Amount]  BIGINT   NOT NULL,
    CONSTRAINT [PK_QZNN_PoolAmountDaily] PRIMARY KEY CLUSTERED ([LogDate] ASC)
);

