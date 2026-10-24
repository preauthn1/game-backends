CREATE TABLE [Stat].[RegUserLog] (
    [Id]              INT      IDENTITY (1, 1) NOT NULL,
    [TotalRegCount]   INT      NOT NULL,
    [IOSRegCount]     INT      NOT NULL,
    [AndroidRegCount] INT      NOT NULL,
    [PCRegCount]      INT      NOT NULL,
    [PlatformID]      INT      CONSTRAINT [DF_RegUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]        DATETIME NOT NULL,
    [AreaID]          INT      NULL,
    CONSTRAINT [PK_RegUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_RegUserLog_StatTime]
    ON [Stat].[RegUserLog]([StatTime] ASC);

