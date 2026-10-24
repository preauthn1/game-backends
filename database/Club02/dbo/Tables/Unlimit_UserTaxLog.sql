CREATE TABLE [dbo].[Unlimit_UserTaxLog] (
    [ID]          BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]     DATETIME NOT NULL,
    [LogTime]     INT      NOT NULL,
    [UserID]      INT      NOT NULL,
    [TaxAmount]   BIGINT   NOT NULL,
    [BonusAmount] BIGINT   NULL,
    [CreateTime]  DATETIME NOT NULL,
    [RelationID]  INT      NULL,
    CONSTRAINT [PK_Unlimit_UserTaxLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

