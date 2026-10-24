CREATE TABLE [dbo].[Unlimit_WithdrawBonus] (
    [ID]         BIGINT   IDENTITY (1, 1) NOT NULL,
    [RelationID] BIGINT   NOT NULL,
    [UserID]     INT      NOT NULL,
    [Amount]     BIGINT   NOT NULL,
    [Status]     INT      NOT NULL,
    [UpdateTime] DATETIME NULL,
    CONSTRAINT [PK_Unlimit_WithdrawBonus] PRIMARY KEY CLUSTERED ([ID] ASC)
);

