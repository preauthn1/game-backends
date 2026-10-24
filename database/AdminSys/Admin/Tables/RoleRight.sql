CREATE TABLE [Admin].[RoleRight] (
    [Id]      INT IDENTITY (637, 1) NOT NULL,
    [RoleID]  INT NOT NULL,
    [RightID] INT NOT NULL,
    CONSTRAINT [PK_RoleRight] PRIMARY KEY CLUSTERED ([RoleID] ASC, [RightID] ASC)
);

