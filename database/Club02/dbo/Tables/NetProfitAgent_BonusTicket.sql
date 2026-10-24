CREATE TABLE [dbo].[NetProfitAgent_BonusTicket] (
    [ID]          BIGINT        IDENTITY (1, 1) NOT NULL,
    [LogDate]     DATETIME      NOT NULL,
    [LogHour]     INT           NOT NULL,
    [UserID]      INT           NOT NULL,
    [BonusAmount] BIGINT        NULL,
    [Relations]   VARCHAR (MAX) NULL,
    CONSTRAINT [PK_NetProfitAgent_BonusTicket] PRIMARY KEY CLUSTERED ([ID] ASC)
);

