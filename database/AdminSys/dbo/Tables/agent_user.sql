CREATE TABLE [dbo].[agent_user] (
    [id]             INT            NOT NULL,
    [telphone]       BIGINT         CONSTRAINT [DF__agent_use__telph__14B10FFA] DEFAULT ((0)) NOT NULL,
    [qq]             INT            CONSTRAINT [DF__agent_user__qq__15A53433] DEFAULT ((0)) NOT NULL,
    [weixin]         NVARCHAR (50)  CONSTRAINT [DF__agent_use__weixi__1699586C] DEFAULT ('') NOT NULL,
    [name]           NVARCHAR (50)  CONSTRAINT [DF__agent_user__name__178D7CA5] DEFAULT ('') NOT NULL,
    [password]       NVARCHAR (100) NOT NULL,
    [encrypt]        NVARCHAR (20)  NOT NULL,
    [created_at]     DATETIME       CONSTRAINT [DF__agent_use__creat__1881A0DE] DEFAULT (getdate()) NULL,
    [updated_at]     DATETIME       CONSTRAINT [DF__agent_use__updat__1975C517] DEFAULT (getdate()) NULL,
    [parent_id]      INT            CONSTRAINT [DF__agent_use__paren__1A69E950] DEFAULT ((0)) NOT NULL,
    [player_id]      INT            CONSTRAINT [DF__agent_use__playe__1B5E0D89] DEFAULT ((0)) NOT NULL,
    [channel]        INT            CONSTRAINT [DF__agent_use__chann__1C5231C2] DEFAULT ((0)) NOT NULL,
    [status]         TINYINT        CONSTRAINT [DF__agent_use__statu__1D4655FB] DEFAULT ((1)) NOT NULL,
    [level]          SMALLINT       CONSTRAINT [DF__agent_use__level__1E3A7A34] DEFAULT ((5)) NOT NULL,
    [min_money]      INT            CONSTRAINT [DF__agent_use__min_m__1F2E9E6D] DEFAULT ((100)) NOT NULL,
    [telphone_bind]  TINYINT        CONSTRAINT [DF__agent_use__telph__2022C2A6] DEFAULT ((0)) NOT NULL,
    [money]          INT            CONSTRAINT [DF__agent_use__money__2116E6DF] DEFAULT ((0)) NOT NULL,
    [ip]             NVARCHAR (20)  CONSTRAINT [DF__agent_user__ip__220B0B18] DEFAULT ('') NOT NULL,
    [area]           NVARCHAR (50)  CONSTRAINT [DF__agent_user__area__22FF2F51] DEFAULT ('') NOT NULL,
    [remark]         NVARCHAR (255) CONSTRAINT [DF__agent_use__remar__23F3538A] DEFAULT ('') NOT NULL,
    [total_recharge] INT            CONSTRAINT [DF__agent_use__total__24E777C3] DEFAULT ((0)) NOT NULL,
    [total_turn_in]  INT            CONSTRAINT [DF__agent_use__total__25DB9BFC] DEFAULT ((0)) NOT NULL,
    [total_turn_out] INT            CONSTRAINT [DF__agent_use__total__26CFC035] DEFAULT ((0)) NOT NULL,
    [total_exchange] INT            CONSTRAINT [DF__agent_use__total__27C3E46E] DEFAULT ((0)) NOT NULL,
    [clock]          TINYINT        CONSTRAINT [DF__agent_use__clock__28B808A7] DEFAULT ((0)) NOT NULL,
    [auto_reply]     NVARCHAR (255) CONSTRAINT [DF__agent_use__auto___29AC2CE0] DEFAULT ('') NOT NULL,
    [transfer]       TINYINT        CONSTRAINT [DF__agent_use__trans__2AA05119] DEFAULT ((0)) NOT NULL,
    [session_id]     NVARCHAR (50)  CONSTRAINT [DF__agent_use__sessi__2B947552] DEFAULT ('') NOT NULL,
    [alipay]         NVARCHAR (50)  CONSTRAINT [DF_agent_user_alipay] DEFAULT ('') NOT NULL,
    [jiuliao]        NVARCHAR (50)  CONSTRAINT [DF_agent_user_jiuliao_1] DEFAULT ('') NOT NULL,
    [secretkey]      VARCHAR (20)   CONSTRAINT [DF_agent_user_secretkey] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK__agent_us__3213E83F63D8CE75] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'手机号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'telphone';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'QQ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'qq';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'微信号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'weixin';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理名称', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'密码', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'password';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'动态密钥', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'encrypt';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'更新时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'updated_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'上次代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'parent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家渠道号', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'channel';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态0禁用1正常', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理等级1-5', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'level';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'最低代充金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'min_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'绑定手机1是0否', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'telphone_bind';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'金钱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'注册IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'ip';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'地区', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'area';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'备注', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'remark';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总充值', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'total_recharge';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总转入', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'total_turn_in';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总转出', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'total_turn_out';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总兑换', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'total_exchange';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'打卡0未打卡1已打卡2永久', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'clock';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'自动回复内容', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'auto_reply';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'快速转账', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'transfer';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'会话ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_user', @level2type = N'COLUMN', @level2name = N'session_id';

