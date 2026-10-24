CREATE TABLE [ServerInfo].[GameServerList] (
    [ServerID]       INT          NOT NULL,
    [GameID]         INT          NOT NULL,
    [ServerName]     VARCHAR (64) NOT NULL,
    [ShowName]       VARCHAR (64) NOT NULL,
    [ClientAddr]     VARCHAR (32) NULL,
    [NotifyAddr]     VARCHAR (32) NULL,
    [HttpAddr]       VARCHAR (32) NULL,
    [Money]          BIGINT       NOT NULL,
    [LastUpdateTime] DATETIME     NOT NULL,
    CONSTRAINT [PK_GameServerList] PRIMARY KEY CLUSTERED ([ServerID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_GameServerList_GameID_ServerName]
    ON [ServerInfo].[GameServerList]([GameID] ASC, [ServerName] ASC);

