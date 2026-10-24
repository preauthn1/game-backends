CREATE TABLE [Admin].[QuickAction] (
    [Id]     INT            IDENTITY (1, 1) NOT NULL,
    [UserID] INT            NOT NULL,
    [Name]   VARCHAR (64)   NOT NULL,
    [Url]    VARCHAR (1024) NOT NULL,
    [Sort]   INT            NOT NULL,
    CONSTRAINT [PK_QuickAction] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_QuickAction_UserID]
    ON [Admin].[QuickAction]([UserID] ASC);

