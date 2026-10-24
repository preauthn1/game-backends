CREATE TABLE [Area].[AreaAdminUser] (
    [AdminID]    INT NOT NULL,
    [PlatformID] INT NOT NULL,
    [AreaID]     INT NOT NULL,
    CONSTRAINT [PK_AreaAdminUser] PRIMARY KEY CLUSTERED ([AdminID] ASC, [PlatformID] ASC, [AreaID] ASC)
);

