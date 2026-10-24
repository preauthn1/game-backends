CREATE TABLE [Stat].[StatisticsRegUser] (
    [Id]                  INT      IDENTITY (1, 1) NOT NULL,
    [StatTime]            DATETIME NOT NULL,
    [PlatformID]          INT      CONSTRAINT [DF_RegUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [AreaID]              INT      NULL,
    [TotalRegCount]       INT      CONSTRAINT [DF_StatisticsRegUser_TotalRegCount] DEFAULT ((0)) NULL,
    [IOSRegCount]         INT      NOT NULL,
    [AndroidRegCount]     INT      NOT NULL,
    [PCRegCount]          INT      NOT NULL,
    [LoginAppUser]        INT      NULL,
    [LoginAppIOSUser]     INT      NULL,
    [LoginAppAndroidUser] INT      NULL,
    [LoginAppPCUser]      INT      NULL,
    CONSTRAINT [PK_RegUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

