CREATE TABLE [Stat].[LogonUserLog] (
    [Id]                INT      IDENTITY (1, 1) NOT NULL,
    [TotalLogonCount]   INT      NOT NULL,
    [PCLogonCount]      INT      NOT NULL,
    [IOSLogonCount]     INT      NOT NULL,
    [AndroidLogonCount] INT      NOT NULL,
    [PlatformID]        INT      CONSTRAINT [DF_LogonUserLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [StatTime]          DATETIME NOT NULL,
    CONSTRAINT [PK_LogonUserLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_LogonUserLog_StatTime]
    ON [Stat].[LogonUserLog]([StatTime] ASC);

