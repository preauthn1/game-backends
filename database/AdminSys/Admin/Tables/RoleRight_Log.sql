CREATE TABLE [Admin].[RoleRight_Log] (
    [LogID]   INT IDENTITY (1, 1) NOT NULL,
    [Id]      INT NOT NULL,
    [RoleID]  INT NOT NULL,
    [RightID] INT NOT NULL,
    CONSTRAINT [PK_RoleRight_Log] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

