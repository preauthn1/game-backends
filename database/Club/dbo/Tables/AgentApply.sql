CREATE TABLE [dbo].[AgentApply] (
    [ApplyID]    UNIQUEIDENTIFIER CONSTRAINT [DF_AgentApply_ApplyID] DEFAULT (newid()) NOT NULL,
    [ID]         UNIQUEIDENTIFIER NOT NULL,
    [UserID]     INT              NOT NULL,
    [LoginName]  VARCHAR (32)     NOT NULL,
    [PlatformID] INT              NOT NULL,
    [AreaID]     INT              NOT NULL,
    [ParentID]   UNIQUEIDENTIFIER NOT NULL,
    [ApplyClass] TINYINT          NOT NULL,
    [Status]     TINYINT          CONSTRAINT [DF_AgentApply_Status] DEFAULT ((0)) NOT NULL,
    [Memo]       VARCHAR (50)     NULL,
    [CreateTime] DATETIME         CONSTRAINT [DF_AgentApply_CreateTime] DEFAULT (getdate()) NOT NULL,
    [Tel]        VARCHAR (20)     NULL,
    [Weixin]     VARCHAR (50)     NULL,
    [QQ]         VARCHAR (50)     NULL,
    [Email]      VARCHAR (50)     NULL,
    CONSTRAINT [PK_AgentApply_1] PRIMARY KEY CLUSTERED ([ApplyID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'申請者正式帳號', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'LoginName';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'申請者平台號', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'PlatformID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'申請者渠道號', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'AreaID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'所屬的上級代理編號', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'ParentID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0待審核 1審核通過 2审核未通过', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'審核註解', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'Memo';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'申請時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentApply', @level2type = N'COLUMN', @level2name = N'CreateTime';

