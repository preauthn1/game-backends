CREATE TABLE [Game].[SuperTool_VIPUser] (
    [UserID]          INT          NOT NULL,
    [AssignAmount]    BIGINT       CONSTRAINT [DF_SuperTool_VIPUser_AssignAmount] DEFAULT ((0)) NOT NULL,
    [AssignRate]      INT          NULL,
    [CompleteAmount]  BIGINT       CONSTRAINT [DF_SuperTool_VIPUser_CompleteAmount] DEFAULT ((0)) NOT NULL,
    [Complete]        TINYINT      NULL,
    [Status]          TINYINT      NULL,
    [Operator]        VARCHAR (50) NULL,
    [SumAssignAmount] BIGINT       CONSTRAINT [DF_SuperTool_VIPUser_SumAssignAmount] DEFAULT ((0)) NOT NULL,
    [UpdateTime]      DATETIME     CONSTRAINT [DF_SuperTool_VIPUser_UpdateTime] DEFAULT (getdate()) NOT NULL,
    [FinishTime]      DATETIME     NULL,
    [CreateTime]      DATETIME     CONSTRAINT [DF_SuperTool_VIPUser_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SuperTool_VIPUser_1] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'指定放分金额', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_VIPUser', @level2type = N'COLUMN', @level2name = N'AssignAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'放分胜率', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_VIPUser', @level2type = N'COLUMN', @level2name = N'AssignRate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'完成金额', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_VIPUser', @level2type = N'COLUMN', @level2name = N'CompleteAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否完成: 1完成 0否', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_VIPUser', @level2type = N'COLUMN', @level2name = N'Complete';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'开始暂停按钮 1 開始 0 暫停', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_VIPUser', @level2type = N'COLUMN', @level2name = N'Status';

