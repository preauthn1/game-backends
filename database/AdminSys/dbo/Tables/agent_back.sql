CREATE TABLE [dbo].[agent_back] (
    [id]              INT      NOT NULL,
    [total_agent_id]  INT      NOT NULL,
    [money]           INT      CONSTRAINT [DF__agent_bac__money__16CE6296] DEFAULT ((0)) NOT NULL,
    [agent_old_money] INT      CONSTRAINT [DF__agent_bac__agent__17C286CF] DEFAULT ((0)) NOT NULL,
    [agent_new_money] INT      CONSTRAINT [DF__agent_bac__agent__18B6AB08] DEFAULT ((0)) NOT NULL,
    [created_at]      DATETIME CONSTRAINT [DF__agent_bac__creat__19AACF41] DEFAULT (getdate()) NULL,
    [updated_at]      DATETIME CONSTRAINT [DF__agent_bac__updat__1A9EF37A] DEFAULT (getdate()) NULL,
    [agent_id]        INT      NOT NULL,
    [player_id]       INT      CONSTRAINT [DF__agent_bac__playe__1B9317B3] DEFAULT ((0)) NOT NULL,
    [error_user_id]   INT      CONSTRAINT [DF__agent_bac__error__1C873BEC] DEFAULT ((0)) NOT NULL,
    [status]          TINYINT  CONSTRAINT [DF__agent_bac__statu__1D7B6025] DEFAULT ((0)) NOT NULL,
    [type]            TINYINT  CONSTRAINT [DF__agent_back__type__1E6F845E] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK__agent_ba__3213E83F3335971A] PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理回调表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'总代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'total_agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'订单金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理退分前金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'agent_old_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理退分后金额', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'agent_new_money';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'创建时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'created_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'修改时间', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'updated_at';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'agent_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'player_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'转错玩家ID或者代理ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'error_user_id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态0未处理1已处理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'类型1代理2玩家', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_back', @level2type = N'COLUMN', @level2name = N'type';

