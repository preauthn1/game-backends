CREATE TABLE [Log].[HDReg] (
    [HDType]     TINYINT      NOT NULL,
    [HDCode]     VARCHAR (64) NOT NULL,
    [RegCount]   INT          NOT NULL,
    [LogDate]    INT          NOT NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_HDReg_CreateTime] DEFAULT (getdate()) NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_HDReg]
    ON [Log].[HDReg]([HDCode] ASC, [LogDate] ASC);

