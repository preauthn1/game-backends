CREATE TABLE [Event].[ContestEventLog] (
    [id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserID]     INT            NOT NULL,
    [Data]       VARCHAR (1000) NULL,
    [CreateTime] DATETIME       CONSTRAINT [DF_ContestEventLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ContestEventLog] PRIMARY KEY CLUSTERED ([id] ASC)
);

