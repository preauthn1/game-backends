CREATE TABLE [ServerInfo].[DomainConfig] (
    [Payment]  VARCHAR (2048) NOT NULL,
    [DLUpdate] VARCHAR (2048) NOT NULL,
    [Club]     VARCHAR (500)  NOT NULL,
    [Customer] VARCHAR (500)  NOT NULL,
    [Company]  INT            NOT NULL,
    CONSTRAINT [PK_DomainConfig] PRIMARY KEY CLUSTERED ([Company] ASC)
);

