CREATE TABLE [Warning].[WarningLog] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [TypeID]     INT           NOT NULL,
    [Memo]       VARCHAR (128) NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_WarningLog] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 80)
);


GO
CREATE NONCLUSTERED INDEX [IX_WarningLog_CreateTime]
    ON [Warning].[WarningLog]([CreateTime] ASC);

