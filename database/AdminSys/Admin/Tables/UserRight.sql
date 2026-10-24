CREATE TABLE [Admin].[UserRight] (
    [Id]      INT IDENTITY (186, 1) NOT NULL,
    [UserID]  INT NOT NULL,
    [RightID] INT NOT NULL,
    CONSTRAINT [PK_UserRight] PRIMARY KEY CLUSTERED ([UserID] ASC, [RightID] ASC)
);

