CREATE TABLE [FK].[ApplyStatus] (
    [Status]      TINYINT      NOT NULL,
    [Description] VARCHAR (50) NOT NULL,
    CONSTRAINT [PK_ApplyStatus] PRIMARY KEY CLUSTERED ([Status] ASC)
);

