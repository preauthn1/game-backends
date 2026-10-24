CREATE TABLE [dbo].[agent_login_total] (
    [id]         INT           IDENTITY (1, 1) NOT NULL,
    [agent_id]   INT           NOT NULL,
    [remark]     NVARCHAR (50) CONSTRAINT [DF__agent_log__remar__42ACE4D4] DEFAULT ('') NOT NULL,
    [created_at] DATETIME      CONSTRAINT [DF__agent_log__creat__43A1090D] DEFAULT (getdate()) NULL,
    [ip]         NVARCHAR (20) CONSTRAINT [DF__agent_login___ip__44952D46] DEFAULT ('') NOT NULL,
    [area]       NVARCHAR (50) CONSTRAINT [DF__agent_logi__area__4589517F] DEFAULT ('') NULL,
    CONSTRAINT [PK__agent_lo__3213E83F3EA749C6] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理登录日志', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_login_total';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_login_total', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_login_total', @level2type = N'COLUMN', @level2name = N'remark';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_login_total', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_login_total', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'地区', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_login_total', @level2type = N'COLUMN', @level2name = N'area';

