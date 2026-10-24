CREATE TABLE [Game].[GameMessage] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [PlatformID]   INT           CONSTRAINT [DF_GameMessage_PlatformID] DEFAULT ((1)) NOT NULL,
    [MsgContent]   VARCHAR (512) NOT NULL,
    [BeginTime]    DATETIME      NOT NULL,
    [EndTime]      DATETIME      NOT NULL,
    [Interval]     INT           NOT NULL,
    [Status]       TINYINT       NOT NULL,
    [LastSendTime] DATETIME      NULL,
    [AllPlatform]  TINYINT       CONSTRAINT [DF_GameMessage_AllPlatform_1] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_GameMessage] PRIMARY KEY CLUSTERED ([Id] ASC)
);

