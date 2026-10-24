CREATE TABLE [Robot].[ServerConfig] (
    [Id]          INT      IDENTITY (1, 1) NOT NULL,
    [ServerID]    INT      NOT NULL,
    [BeginTime]   CHAR (5) NOT NULL,
    [EndTime]     CHAR (5) NOT NULL,
    [MinCount]    INT      NOT NULL,
    [MaxCount]    INT      NOT NULL,
    [MinLevel]    INT      NOT NULL,
    [MaxLevel]    INT      NOT NULL,
    [StartMinute] INT      NULL,
    [EndMinute]   INT      NULL,
    [Status]      TINYINT  NULL,
    CONSTRAINT [PK_ServerConfig] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ServerConfig_ServerID]
    ON [Robot].[ServerConfig]([ServerID] ASC);

