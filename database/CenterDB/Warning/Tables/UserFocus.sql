CREATE TABLE [Warning].[UserFocus] (
    [Id]     INT IDENTITY (1, 1) NOT NULL,
    [TypeID] INT NOT NULL,
    [UserID] INT NOT NULL,
    CONSTRAINT [PK_UserFocus] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 80)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_UserFocus_TypeID_UserID]
    ON [Warning].[UserFocus]([UserID] ASC, [TypeID] ASC);

