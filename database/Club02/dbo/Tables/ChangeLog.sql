CREATE TABLE [dbo].[ChangeLog] (
    [LogID]      UNIQUEIDENTIFIER CONSTRAINT [DF_ChangeLog_LogID] DEFAULT (newid()) NOT NULL,
    [ChangeType] TINYINT          NOT NULL,
    [OperateID]  UNIQUEIDENTIFIER NOT NULL,
    [TargetID]   UNIQUEIDENTIFIER NULL,
    [OldData]    VARCHAR (500)    NOT NULL,
    [NewData]    VARCHAR (500)    NOT NULL,
    [Memo]       VARCHAR (50)     NOT NULL,
    [CreateTime] DATETIME         CONSTRAINT [DF_UserLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ChangeLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'使用者UID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChangeLog', @level2type = N'COLUMN', @level2name = N'OperateID';

