CREATE TABLE [Game].[QZNN_SystemPool] (
    [FlagID] INT    NOT NULL,
    [Amount] BIGINT CONSTRAINT [DF_QZNN_SystemPool_Amount] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_QZNN_SystemPool] PRIMARY KEY CLUSTERED ([FlagID] ASC)
);

