CREATE TABLE [dbo].[agent_money_total] (
    [id]             INT            IDENTITY (1, 1) NOT NULL,
    [total_agent_id] INT            NOT NULL,
    [type]           TINYINT        CONSTRAINT [DF__agent_mone__type__52E34C9D] DEFAULT ((0)) NOT NULL,
    [user_agent_id]  INT            CONSTRAINT [DF__agent_mon__user___53D770D6] DEFAULT ((0)) NOT NULL,
    [money]          INT            CONSTRAINT [DF__agent_mon__money__54CB950F] DEFAULT ((0)) NOT NULL,
    [old_money]      INT            CONSTRAINT [DF__agent_mon__old_m__55BFB948] DEFAULT ((0)) NOT NULL,
    [new_money]      INT            CONSTRAINT [DF__agent_mon__new_m__56B3DD81] DEFAULT ((0)) NOT NULL,
    [created_at]     DATETIME       CONSTRAINT [DF__agent_mon__creat__57A801BA] DEFAULT (getdate()) NULL,
    [ip]             NVARCHAR (20)  CONSTRAINT [DF__agent_money___ip__589C25F3] DEFAULT ('') NOT NULL,
    [area]           NVARCHAR (50)  CONSTRAINT [DF__agent_mone__area__59904A2C] DEFAULT ('') NULL,
    [device]         TINYINT        CONSTRAINT [DF__agent_mon__devic__5A846E65] DEFAULT ((1)) NOT NULL,
    [remark]         NVARCHAR (255) CONSTRAINT [DF__agent_mon__remar__5B78929E] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK__agent_mo__3213E83F473C8FC7] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理金额变化', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'total_agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账类型0转出1转入2充值4回调', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'user_agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账前金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'old_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账后金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'new_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转账时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作地区', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'area';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作终端设备(1-PC|2-手机)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'device';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_money_total', @level2type = N'COLUMN', @level2name = N'remark';

