CREATE TABLE [Users].[EventSetting] (
    [ID]         INT          IDENTITY (1, 1) NOT NULL,
    [EventID]    SMALLINT     NOT NULL,
    [EventName]  VARCHAR (32) NOT NULL,
    [StartTime]  DATETIME     NOT NULL,
    [EndTime]    DATETIME     NOT NULL,
    [Status]     INT          CONSTRAINT [DF_EventSetting_Status] DEFAULT ((1)) NOT NULL,
    [GiftCash]   BIGINT       CONSTRAINT [DF_EventSetting_GiftCash] DEFAULT ((0)) NOT NULL,
    [Updater]    VARCHAR (20) NULL,
    [UpdateTime] DATETIME     NULL,
    CONSTRAINT [PK_EventSetting_1] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventSetting_EventName]
    ON [Users].[EventSetting]([EventName] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UK_EventSetting_EventID]
    ON [Users].[EventSetting]([EventID] ASC)
    INCLUDE([EventName], [StartTime], [EndTime], [Status], [GiftCash]);

