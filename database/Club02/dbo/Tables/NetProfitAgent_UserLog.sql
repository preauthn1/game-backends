CREATE TABLE [dbo].[NetProfitAgent_UserLog] (
    [LogDate]      DATETIME NOT NULL,
    [UserID]       INT      NOT NULL,
    [WalletAmount] BIGINT   CONSTRAINT [DF_NetProfitAgentUserLog_WalletAmount] DEFAULT ((0)) NULL,
    [BankAmount]   BIGINT   CONSTRAINT [DF_NetProfitAgentUserLog_BankAmount] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_NetProfitAgent_UserLog] PRIMARY KEY CLUSTERED ([LogDate] ASC, [UserID] ASC)
);

