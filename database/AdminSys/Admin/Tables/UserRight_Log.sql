CREATE TABLE [Admin].[UserRight_Log] (
    [LogID]   INT IDENTITY (1, 1) NOT NULL,
    [Id]      INT NOT NULL,
    [UserID]  INT NOT NULL,
    [RightID] INT NOT NULL,
    CONSTRAINT [PK_UserRight_Log] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

