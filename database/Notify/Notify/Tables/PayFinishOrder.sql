CREATE TABLE [Notify].[PayFinishOrder] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [TimeSeq]    BIGINT   NULL,
    [OrderID]    INT      NOT NULL,
    [Amount]     BIGINT   NOT NULL,
    [UserID]     INT      NOT NULL,
    [CreateTime] DATETIME CONSTRAINT [DF_PayFinishOrder_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_PayFinishOrder_1] PRIMARY KEY CLUSTERED ([ID] ASC)
);

