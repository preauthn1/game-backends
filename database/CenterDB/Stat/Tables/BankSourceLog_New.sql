CREATE TABLE [Stat].[BankSourceLog_New] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [PlatformID] INT      NOT NULL,
    [AreaID]     INT      NOT NULL,
    [SourceID]   INT      NOT NULL,
    [Amount]     BIGINT   NOT NULL,
    [StatTime]   DATETIME NOT NULL,
    CONSTRAINT [PK_BankSourceLog_New] PRIMARY KEY CLUSTERED ([Id] ASC)
);

