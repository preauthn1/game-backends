CREATE TABLE [ServerInfo].[Config] (
    [Name]  VARCHAR (250) NOT NULL,
    [Value] VARCHAR (250) NOT NULL,
    [Memo]  VARCHAR (250) NULL,
    CONSTRAINT [PK_Config] PRIMARY KEY CLUSTERED ([Name] ASC)
);

