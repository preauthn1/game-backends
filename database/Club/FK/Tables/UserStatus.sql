CREATE TABLE [FK].[UserStatus] (
    [Status]      TINYINT      NOT NULL,
    [Description] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_UserStatus] PRIMARY KEY CLUSTERED ([Status] ASC)
);

