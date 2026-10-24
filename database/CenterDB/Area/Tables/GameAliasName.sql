CREATE TABLE [Area].[GameAliasName] (
    [GameID]    INT          NOT NULL,
    [IconID]    INT          IDENTITY (1, 1) NOT NULL,
    [IconName]  VARCHAR (50) NULL,
    [IconTitle] VARCHAR (50) NULL,
    [Skin]      VARCHAR (50) NULL,
    [AliasName] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_GameAliasName] PRIMARY KEY CLUSTERED ([IconID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_GameAliasName_GameID]
    ON [Area].[GameAliasName]([GameID] ASC);

