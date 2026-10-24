CREATE TABLE [Game].[SuperTool_ChangeLog] (
    [LogID]      INT           IDENTITY (1, 1) NOT NULL,
    [GameID]     INT           NOT NULL,
    [ServerID]   INT           NOT NULL,
    [TableNo]    INT           NOT NULL,
    [Type]       TINYINT       NOT NULL,
    [Result]     VARCHAR (500) NULL,
    [Operator]   VARCHAR (50)  NULL,
    [CreateTime] DATETIME      CONSTRAINT [DF_SuperTool_PoolLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SuperTool_ChangeLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_SuperTool_ChangeLog]
    ON [Game].[SuperTool_ChangeLog]([GameID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1: 編輯獎池金額 2:增加放分金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_ChangeLog', @level2type = N'COLUMN', @level2name = N'Type';

