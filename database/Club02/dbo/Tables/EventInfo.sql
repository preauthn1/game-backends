CREATE TABLE [dbo].[EventInfo] (
    [EventID]    INT          IDENTITY (1, 1) NOT NULL,
    [EventName]  VARCHAR (50) NOT NULL,
    [CreateTime] DATETIME     NOT NULL,
    CONSTRAINT [PK_EventInfo] PRIMARY KEY CLUSTERED ([EventID] ASC)
);

