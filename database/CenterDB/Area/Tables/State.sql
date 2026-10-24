CREATE TABLE [Area].[State] (
    [Id]       INT           IDENTITY (1, 1) NOT NULL,
    [AreaName] VARCHAR (255) NOT NULL,
    CONSTRAINT [PK_State] PRIMARY KEY CLUSTERED ([Id] ASC)
);

