CREATE TABLE [dbo].[Unlimit_UserLogin] (
    [LogDate]    DATETIME NOT NULL,
    [UserID]     INT      NOT NULL,
    [LoginCount] INT      NULL,
    CONSTRAINT [PK_Unlimit_UserLogin] PRIMARY KEY CLUSTERED ([LogDate] ASC, [UserID] ASC)
);

