CREATE TABLE [dbo].[EventPlatformInfo] (
    [PlatformID]     INT      NOT NULL,
    [AreaID]         INT      NOT NULL,
    [EventID]        INT      NOT NULL,
    [EventStartDate] DATETIME NOT NULL,
    [EventEndDate]   DATETIME NOT NULL,
    [Status]         INT      NOT NULL,
    CONSTRAINT [PK_EventPlatformInfo] PRIMARY KEY CLUSTERED ([PlatformID] ASC, [AreaID] ASC, [EventID] ASC)
);

