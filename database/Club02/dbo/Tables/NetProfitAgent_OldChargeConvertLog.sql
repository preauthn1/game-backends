CREATE TABLE [dbo].[NetProfitAgent_OldChargeConvertLog] (
    [UserID]           INT    NOT NULL,
    [OldChargeAmount]  BIGINT CONSTRAINT [DF_NetProfitAgent_OldChargeConvertLog_OldChargeAmount] DEFAULT ((0)) NOT NULL,
    [OldConvertAmount] BIGINT CONSTRAINT [DF_NetProfitAgent_OldChargeConvertLog_OldConvertAmount] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_NetProfitAgent_OldChargeConvertLog] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

