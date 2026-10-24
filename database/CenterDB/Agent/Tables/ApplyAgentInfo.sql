CREATE TABLE [Agent].[ApplyAgentInfo] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [UserID]     INT           CONSTRAINT [DF_ApplyAgentInfo_UserID] DEFAULT ((0)) NOT NULL,
    [PlatformID] INT           CONSTRAINT [DF_ApplyAgentInfo_PlatformID] DEFAULT ((1)) NOT NULL,
    [Name]       VARCHAR (32)  NOT NULL,
    [Tel]        VARCHAR (20)  NOT NULL,
    [QQ]         VARCHAR (20)  NOT NULL,
    [WXNo]       VARCHAR (20)  NOT NULL,
    [Alipay]     VARCHAR (50)  CONSTRAINT [DF_ApplyAgentInfo_Alipay] DEFAULT ('') NOT NULL,
    [Memo]       VARCHAR (256) NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_ApplyAgentInfo_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ApplyAgentInfo_CreateTime]
    ON [Agent].[ApplyAgentInfo]([CreateTime] ASC);

