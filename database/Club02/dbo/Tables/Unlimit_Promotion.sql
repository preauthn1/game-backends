CREATE TABLE [dbo].[Unlimit_Promotion] (
    [UserID]        INT NOT NULL,
    [SubUser]       INT NULL,
    [SubDirectUser] INT NULL,
    [NewUser]       INT NULL,
    [DAU]           INT NULL,
    CONSTRAINT [PK_Unlimit_Promotion_1] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

