CREATE TABLE [dbo].[agent_apply] (
    [id]         INT            IDENTITY (1, 1) NOT NULL,
    [player_id]  INT            NOT NULL,
    [channel]    INT            DEFAULT ((0)) NOT NULL,
    [name]       NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [qq]         INT            DEFAULT ((0)) NOT NULL,
    [weixin]     NVARCHAR (50)  DEFAULT ('') NOT NULL,
    [telphone]   BIGINT         DEFAULT ((0)) NOT NULL,
    [status]     TINYINT        DEFAULT ((0)) NOT NULL,
    [reason]     NVARCHAR (255) DEFAULT ('') NOT NULL,
    [created_at] DATETIME       DEFAULT (getdate()) NULL,
    [updated_at] DATETIME       DEFAULT (getdate()) NULL,
    [reply]      NVARCHAR (255) DEFAULT ('') NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理申请表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家渠道', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'channel';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'申请人', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'qq', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'qq';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'微信', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'weixin';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'telphone';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'审核状态0未审核1审核通过2拒绝申请', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'申请理由', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'reason';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'修改时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'updated_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'回复内容', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_apply', @level2type = N'COLUMN', @level2name = N'reply';

