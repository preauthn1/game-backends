CREATE TABLE [Admin].[Role] (
    [RoleID]   INT          IDENTITY (1, 1) NOT NULL,
    [RoleName] VARCHAR (32) NOT NULL,
    CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED ([RoleID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_Role_RoleName]
    ON [Admin].[Role]([RoleName] ASC);

