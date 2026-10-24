CREATE TABLE [Game].[ProtectNodeLog] (
    [Id]           INT          IDENTITY (1, 1) NOT NULL,
    [Token]        VARCHAR (10) NOT NULL,
    [UserID]       INT          NOT NULL,
    [Action]       TINYINT      NOT NULL,
    [BackupNodeIP] VARCHAR (32) NULL,
    [NodeIP]       VARCHAR (50) NULL,
    [UserIP]       VARCHAR (30) NULL,
    [UserFrom]     VARCHAR (10) NULL,
    [HDCode]       VARCHAR (64) NULL,
    [Status]       TINYINT      CONSTRAINT [DF_ProtectNodeLog_Status] DEFAULT ((0)) NOT NULL,
    [TFServer]     VARCHAR (20) NULL,
    [LoginUserID]  INT          NULL,
    [Memo]         VARCHAR (50) NULL,
    [CreateTime]   DATETIME     CONSTRAINT [DF_ProtectNodeLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProtectNodeLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ProtectNodeLog_NodeIP]
    ON [Game].[ProtectNodeLog]([NodeIP] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ProtectNodeLog_UserID]
    ON [Game].[ProtectNodeLog]([UserID] ASC);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'获取方式: 1 一般 2 直接取得备用节点', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'ProtectNodeLog', @level2type = N'COLUMN', @level2name = N'Action';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'登入状态 0无 1正常登入 2登入失败', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'ProtectNodeLog', @level2type = N'COLUMN', @level2name = N'Status';

