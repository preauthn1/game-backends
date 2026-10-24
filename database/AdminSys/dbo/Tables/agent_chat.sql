CREATE TABLE [dbo].[agent_chat] (
    [id]         INT            IDENTITY (1, 1) NOT NULL,
    [player_id]  INT            DEFAULT ((0)) NOT NULL,
    [agent_id]   INT            DEFAULT ((0)) NOT NULL,
    [msg]        NVARCHAR (255) DEFAULT ('') NOT NULL,
    [reply]      NVARCHAR (255) DEFAULT ('') NOT NULL,
    [created_at] DATETIME       DEFAULT (getdate()) NULL,
    [updated_at] DATETIME       DEFAULT (getdate()) NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'聊天记录', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家消息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat', @level2type = N'COLUMN', @level2name = N'msg';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理回复', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat', @level2type = N'COLUMN', @level2name = N'reply';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_chat', @level2type = N'COLUMN', @level2name = N'updated_at';

