CREATE TABLE [dbo].[AgentUsers] (
    [ID]                 UNIQUEIDENTIFIER CONSTRAINT [DF_AgentUsers_ID] DEFAULT (newid()) NOT NULL,
    [ParentID]           UNIQUEIDENTIFIER NULL,
    [UserID]             INT              NULL,
    [Class]              TINYINT          CONSTRAINT [DF_AgentInfo_UserType] DEFAULT ((0)) NOT NULL,
    [Level]              INT              CONSTRAINT [DF_AgentInfo_Level] DEFAULT ((0)) NOT NULL,
    [LoginName]          VARCHAR (32)     NULL,
    [Password]           VARCHAR (250)    NULL,
    [Encrypt]            VARCHAR (64)     NULL,
    [Status]             TINYINT          CONSTRAINT [DF_AgentUsers_Status] DEFAULT ((1)) NOT NULL,
    [InviteCode]         VARCHAR (20)     NULL,
    [PlatformID]         INT              NULL,
    [AreaID]             INT              NULL,
    [TotalFlows]         BIGINT           CONSTRAINT [DF_AgentUsers_TotalFlows] DEFAULT ((0)) NOT NULL,
    [TotalIncome]        BIGINT           CONSTRAINT [DF_AgentUsers_TotalIncome] DEFAULT ((0)) NOT NULL,
    [UserTotalFlows]     BIGINT           CONSTRAINT [DF_AgentUsers_UserTotalFlows] DEFAULT ((0)) NOT NULL,
    [RemainAmount]       BIGINT           CONSTRAINT [DF_AgentUsers_RemainAmount] DEFAULT ((0)) NOT NULL,
    [CreateTime]         DATETIME         CONSTRAINT [DF_AgentInfo_CreateTime] DEFAULT (getdate()) NOT NULL,
    [UpdateTime]         DATETIME         CONSTRAINT [DF_AgentInfo_UpdateTime] DEFAULT (getdate()) NOT NULL,
    [LastLogonTime]      DATETIME         NULL,
    [LastLogonIPAddress] VARCHAR (15)     NULL,
    [Tel]                VARCHAR (20)     NULL,
    [Weixin]             VARCHAR (50)     NULL,
    [QQ]                 VARCHAR (50)     NULL,
    [Email]              VARCHAR (50)     NULL,
    [DownloadUrl]        VARCHAR (200)    CONSTRAINT [DF_AgentUsers_DownloadUrl] DEFAULT ('http://example.invalid/') NULL,
    [DomainID]           INT              CONSTRAINT [DF_AgentUsers_DomainID] DEFAULT ((0)) NOT NULL,
    [QRUrl]              VARCHAR (200)    NULL,
    [AllowWithdraw]      INT              CONSTRAINT [DF_AgentUsers_AllowWithdraw] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_AgentInfo] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AgentUser_ParentID]
    ON [dbo].[AgentUsers]([ParentID] ASC)
    INCLUDE([ID], [UserID], [Class], [LoginName]);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0: 系統管理者 1:一級代理  2:二級代理 99:一般玩家  100:渠道代理', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'Class';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'等級  ex.(1 王者 2黃金 3鑽石) 可在GrandConfig設定', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'Level';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 關閉; 1開啟; ', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'邀請碼', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'InviteCode';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理的總流水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'TotalFlows';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'代理的總收益', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'TotalIncome';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'用戶自己的總流水', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'AgentUsers', @level2type = N'COLUMN', @level2name = N'UserTotalFlows';

