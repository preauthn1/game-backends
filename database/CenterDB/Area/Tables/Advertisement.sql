CREATE TABLE [Area].[Advertisement] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [PlatformID] INT           NOT NULL,
    [AreaID]     INT           NULL,
    [Picture]    VARCHAR (512) NOT NULL,
    [Link]       VARCHAR (512) NOT NULL,
    [Behavior]   TINYINT       NOT NULL,
    [Status]     TINYINT       NOT NULL,
    [CreateTime] DATETIME      CONSTRAINT [DF_Advertisement_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Advertisement] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Advertisement]
    ON [Area].[Advertisement]([PlatformID] ASC);

