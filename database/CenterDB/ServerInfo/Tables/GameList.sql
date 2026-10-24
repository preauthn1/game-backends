CREATE TABLE [ServerInfo].[GameList] (
    [GameID]         INT            NOT NULL,
    [GameName]       VARCHAR (64)   NOT NULL,
    [AndroidVersion] VARCHAR (1024) NOT NULL,
    [IOSVersion]     VARCHAR (1024) NOT NULL,
    [Status]         TINYINT        NOT NULL,
    [ShortName]      VARCHAR (10)   NULL,
    CONSTRAINT [PK_GameList] PRIMARY KEY CLUSTERED ([GameID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_GameList_GameName]
    ON [ServerInfo].[GameList]([GameName] ASC);

