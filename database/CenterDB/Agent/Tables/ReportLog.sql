CREATE TABLE [Agent].[ReportLog] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [PlatformID] INT           CONSTRAINT [DF_ReportLog_PlatformID] DEFAULT ((1)) NOT NULL,
    [AgentName]  VARCHAR (32)  NOT NULL,
    [Memo]       VARCHAR (MAX) NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_ReportLog_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'举报代理记录', @level0type = N'SCHEMA', @level0name = N'Agent', @level1type = N'TABLE', @level1name = N'ReportLog';

