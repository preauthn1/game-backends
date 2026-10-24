CREATE TABLE [Users].[CSOnBoard] (
    [UserID]     INT      NOT NULL,
    [Status]     INT      NOT NULL,
    [CreateTime] DATETIME NOT NULL,
    [UpdateTime] DATETIME NULL,
    CONSTRAINT [PK_CSOnBoard] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

