CREATE TABLE [ServerInfo].[SMSConfig] (
    [id]     TINYINT      NOT NULL,
    [vender] VARCHAR (20) NOT NULL,
    [status] TINYINT      NOT NULL,
    CONSTRAINT [PK_SMSConfig] PRIMARY KEY CLUSTERED ([id] ASC)
);

