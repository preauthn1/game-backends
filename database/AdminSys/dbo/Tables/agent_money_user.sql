CREATE TABLE [dbo].[agent_money_user] (
    [id]         INT            IDENTITY (1, 1) NOT NULL,
    [agent_id]   INT            NOT NULL,
    [type]       TINYINT        CONSTRAINT [DF__agent_mone__type__5D60DB10] DEFAULT ((0)) NOT NULL,
    [user_id]    INT            CONSTRAINT [DF__agent_mon__user___5E54FF49] DEFAULT ((0)) NOT NULL,
    [money]      INT            CONSTRAINT [DF__agent_mon__money__5F492382] DEFAULT ((0)) NOT NULL,
    [new_money]  INT            CONSTRAINT [DF__agent_mon__new_m__603D47BB] DEFAULT ((0)) NOT NULL,
    [created_at] DATETIME       CONSTRAINT [DF__agent_mon__creat__61316BF4] DEFAULT (getdate()) NULL,
    [ip]         NVARCHAR (20)  CONSTRAINT [DF__agent_money___ip__6225902D] DEFAULT ('') NOT NULL,
    [area]       NVARCHAR (50)  CONSTRAINT [DF__agent_mone__area__6319B466] DEFAULT ('') NULL,
    [status]     TINYINT        CONSTRAINT [DF__agent_mon__statu__640DD89F] DEFAULT ((0)) NOT NULL,
    [remark]     NVARCHAR (240) CONSTRAINT [DF__agent_mon__remar__6501FCD8] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK__agent_mo__3213E83F4A18FC72] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理金额变化', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账类型0转出1转入2购入3兑换4回调', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家或总代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'user_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'交易金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账后金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'new_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作地区', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'area';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态0失败1成功', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_user', @level2type = N'COLUMN', @level2name = N'remark';

