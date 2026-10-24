CREATE TABLE [Event].[TicketJobLog] (
    [ID]            INT      IDENTITY (1, 1) NOT NULL,
    [YYYYMMDD]      INT      NOT NULL,
    [GameID]        INT      NOT NULL,
    [ContestID]     INT      NOT NULL,
    [UserID]        INT      NOT NULL,
    [SumFreeTicket] INT      NOT NULL,
    [IsFull]        TINYINT  NOT NULL,
    [CreateTime]    DATETIME CONSTRAINT [DF_TicketJobLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_TicketJobLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

