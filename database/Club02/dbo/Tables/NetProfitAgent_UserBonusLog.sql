CREATE TABLE [dbo].[NetProfitAgent_UserBonusLog] (
    [ID]            BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]       DATETIME NOT NULL,
    [LogHour]       INT      NOT NULL,
    [UserID]        INT      NOT NULL,
    [ChargeAmount]  BIGINT   NOT NULL,
    [ConvertAmount] BIGINT   NOT NULL,
    [WalletAmount]  BIGINT   NOT NULL,
    [BankAmount]    BIGINT   NOT NULL,
    [UpdateAmount]  BIGINT   NOT NULL,
    [CreateTime]    DATETIME NOT NULL,
    [RelationID]    BIGINT   NULL,
    CONSTRAINT [PK_NetProfitAgent_UserBonusLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

