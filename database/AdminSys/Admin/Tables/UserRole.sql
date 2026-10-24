CREATE TABLE [Admin].[UserRole] (
    [Id]     INT IDENTITY (1, 1) NOT NULL,
    [UserID] INT NOT NULL,
    [RoleID] INT NOT NULL,
    CONSTRAINT [PK_UserRole] PRIMARY KEY CLUSTERED ([UserID] ASC, [RoleID] ASC)
);

