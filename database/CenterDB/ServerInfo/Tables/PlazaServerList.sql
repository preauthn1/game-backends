CREATE TABLE [ServerInfo].[PlazaServerList] (
    [ServerID]       INT          NOT NULL,
    [ClientAddr]     VARCHAR (32) NOT NULL,
    [NotifyAddr]     VARCHAR (32) NOT NULL,
    [HttpAddr]       VARCHAR (32) NULL,
    [LastUpdateTime] DATETIME     NOT NULL,
    CONSTRAINT [PK_PlazaServerList] PRIMARY KEY CLUSTERED ([ServerID] ASC)
);

