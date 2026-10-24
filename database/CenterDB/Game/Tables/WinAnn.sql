CREATE TABLE [Game].[WinAnn] (
    [Id]         INT      IDENTITY (1, 1) NOT NULL,
    [UserID]     INT      NOT NULL,
    [ServerID]   INT      NOT NULL,
    [WinAmount]  BIGINT   NOT NULL,
    [Status]     TINYINT  NOT NULL,
    [CreateTime] DATETIME NOT NULL,
    [SendTime]   DATETIME NULL,
    CONSTRAINT [PK_WinLoseAnn_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'赢金币公告内容', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'WinAnn', @level2type = N'COLUMN', @level2name = N'UserID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态 0 未发　1已发送', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'WinAnn', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'WinAnn', @level2type = N'COLUMN', @level2name = N'CreateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'发送时间', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'WinAnn', @level2type = N'COLUMN', @level2name = N'SendTime';

