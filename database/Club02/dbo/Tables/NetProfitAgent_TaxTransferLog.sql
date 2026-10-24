CREATE TABLE [dbo].[NetProfitAgent_TaxTransferLog] (
    [ID]                    BIGINT   IDENTITY (1, 1) NOT NULL,
    [LogDate]               DATETIME NOT NULL,
    [LogHour]               INT      NOT NULL,
    [UserID]                INT      NOT NULL,
    [Rate]                  INT      NULL,
    [FromUserID]            INT      NULL,
    [FromUserRate]          INT      NULL,
    [Diffrate]              INT      NULL,
    [FromAmount]            BIGINT   NULL,
    [FromAmountWithoutRate] BIGINT   NULL,
    CONSTRAINT [PK_NetProfitAgent_TaxTransferLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

