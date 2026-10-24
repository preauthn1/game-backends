CREATE TABLE [Users].[FocusUser] (
    [UserID] INT NOT NULL,
    CONSTRAINT [PK_FocusUser] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'用于百人类关注用户配置', @level0type = N'SCHEMA', @level0name = N'Users', @level1type = N'TABLE', @level1name = N'FocusUser';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'玩家ID', @level0type = N'SCHEMA', @level0name = N'Users', @level1type = N'TABLE', @level1name = N'FocusUser', @level2type = N'COLUMN', @level2name = N'UserID';

