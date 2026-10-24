CREATE TABLE [dbo].[agent_total] (
    [id]             INT            NOT NULL,
    [username]       NVARCHAR (100) NOT NULL,
    [password]       NVARCHAR (100) NOT NULL,
    [encrypt]        NVARCHAR (20)  NOT NULL,
    [money]          INT            DEFAULT ((0)) NOT NULL,
    [created_at]     DATETIME       DEFAULT (getdate()) NULL,
    [updated_at]     DATETIME       DEFAULT (getdate()) NULL,
    [status]         TINYINT        DEFAULT ((1)) NOT NULL,
    [telphone]       BIGINT         DEFAULT ((0)) NOT NULL,
    [telphone_bind]  TINYINT        DEFAULT ((0)) NOT NULL,
    [telphone_login] TINYINT        DEFAULT ((0)) NOT NULL,
    [ip]             NVARCHAR (20)  DEFAULT ('') NOT NULL,
    [ip_bind]        TINYINT        DEFAULT ((0)) NOT NULL,
    [total_turn_out] INT            DEFAULT ((0)) NOT NULL,
    [total_recharge] INT            DEFAULT ((0)) NOT NULL,
    [secretkey]      VARCHAR (20)   CONSTRAINT [DF_agent_total_secretkey] DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理用户名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'username';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'密码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'password';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'动态密钥', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'encrypt';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'金钱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'updated_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态1启用0禁用', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'手机', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'telphone';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'绑定手机0未绑1绑定', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'telphone_bind';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'手机验证码登录0否1是', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'telphone_login';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'绑定的IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'绑定IP登录0未绑1绑定', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'ip_bind';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总转出', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'total_turn_out';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总充值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_total', @level2type = N'COLUMN', @level2name = N'total_recharge';

