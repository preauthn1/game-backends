CREATE TABLE [dbo].[NetProfitAgent_UserTaxLog] (
    [ID]            BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]       DATETIME NOT NULL,
    [LogTime]       INT      NOT NULL,
    [UserID]        INT      NOT NULL,
    [ChargeAmount]  BIGINT   NOT NULL,
    [ConvertAmount] BIGINT   NOT NULL,
    [WalletAmount]  BIGINT   NOT NULL,
    [BankAmount]    BIGINT   NOT NULL,
    [BonusAmount]   BIGINT   NOT NULL,
    [CreateTime]    DATETIME NOT NULL,
    [RelationID]    INT      NULL,
    CONSTRAINT [PK_NetProfitAgent_UserTaxLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

