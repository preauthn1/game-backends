CREATE TABLE [dbo].[Event_ChargeBonusRecord] (
    [UserID]          INT      NOT NULL,
    [ChargeDate]      DATETIME NOT NULL,
    [ChargeAmount]    BIGINT   NOT NULL,
    [SumFlowAmount]   BIGINT   NOT NULL,
    [FlowAmountLimit] BIGINT   NOT NULL,
    CONSTRAINT [PK_Event_ChargeBonusRecord] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

