CREATE TABLE [Pay].[UnionPlatformPayType] (
    [PlatformID] INT     NOT NULL,
    [GWID]       INT     NOT NULL,
    [TypeID]     INT     NOT NULL,
    [Status]     TINYINT NOT NULL,
    [Popular]    TINYINT NOT NULL,
    CONSTRAINT [PK_UnionPlatformPayType] PRIMARY KEY CLUSTERED ([PlatformID] ASC, [GWID] ASC, [TypeID] ASC)
);

