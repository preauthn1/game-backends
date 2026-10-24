CREATE TABLE [Game].[SuperTool_Fish_VIPUser] (
    [UserID]          INT          NOT NULL,
    [AssignAmount]    BIGINT       NOT NULL,
    [Rate]            INT          NOT NULL,
    [FinishAmount]    BIGINT       NULL,
    [Finish]          TINYINT      CONSTRAINT [DF_SuperTool_Fish_AssignReward_Finish] DEFAULT ((0)) NULL,
    [Status]          TINYINT      NULL,
    [Operator]        VARCHAR (50) NULL,
    [SumAssignAmount] BIGINT       NOT NULL,
    [FinishTime]      DATETIME     NULL,
    [CreateTime]      DATETIME     CONSTRAINT [DF_SuperTool_Fish_AssignReward_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SuperTool_Fish_VIPUser] PRIMARY KEY CLUSTERED ([UserID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'放分金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'AssignAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'完成金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'FinishAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否完成 0未完成  1完成', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'Finish';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 暂停 1 继续放分', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'累計放分金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'SumAssignAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'完成時間', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'FinishTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'放分時間', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_VIPUser', @level2type = N'COLUMN', @level2name = N'CreateTime';

