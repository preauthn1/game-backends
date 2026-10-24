CREATE TABLE [dbo].[agent_menu] (
    [id]     INT            NOT NULL,
    [name]   NVARCHAR (80)  NOT NULL,
    [ename]  NVARCHAR (80)  DEFAULT ('') NOT NULL,
    [url]    NVARCHAR (100) DEFAULT ('') NOT NULL,
    [parent] INT            DEFAULT ((0)) NOT NULL,
    [type]   INT            DEFAULT ((0)) NOT NULL,
    [sort]   INT            DEFAULT ((0)) NOT NULL,
    [status] TINYINT        DEFAULT ((1)) NOT NULL,
    PRIMARY KEY CLUSTERED ([id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'棋牌管理后台菜单表', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'别名', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu', @level2type = N'COLUMN', @level2name = N'ename';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'路由', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu', @level2type = N'COLUMN', @level2name = N'url';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'上级菜单ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu', @level2type = N'COLUMN', @level2name = N'parent';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'类型(0-总代理|1-代理|2-推广员)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu', @level2type = N'COLUMN', @level2name = N'type';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'排序', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu', @level2type = N'COLUMN', @level2name = N'sort';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'状态(1-启用| 0-禁用)', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'agent_menu', @level2type = N'COLUMN', @level2name = N'status';

