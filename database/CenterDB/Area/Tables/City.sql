CREATE TABLE [Area].[City] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [AreaName] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_City] PRIMARY KEY CLUSTERED ([Id] ASC)
);

