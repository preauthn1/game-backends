CREATE TABLE [Game].[SuperTool_Fish_SystemReward] (
    [FlagID]       INT          NOT NULL,
    [FishID]       INT          NOT NULL,
    [Amount]       BIGINT       NOT NULL,
    [FinishAmount] BIGINT       CONSTRAINT [DF_SuperTool_Fish_SystemReward_FinishAmount] DEFAULT ((0)) NOT NULL,
    [Rate]         INT          NOT NULL,
    [Status]       TINYINT      CONSTRAINT [DF_SuperTool_Fish_SystemReward_Status] DEFAULT ((0)) NOT NULL,
    [Finish]       TINYINT      CONSTRAINT [DF_SuperTool_Fish_SystemReward_Finish] DEFAULT ((0)) NOT NULL,
    [FinishTime]   DATETIME     NULL,
    [Operator]     VARCHAR (50) NULL,
    [CreateTime]   DATETIME     NULL,
    CONSTRAINT [PK_SuperTool_Fish_SystemReward] PRIMARY KEY CLUSTERED ([FlagID] ASC, [FishID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'魚編號', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'FishID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'放分金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'Amount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'完成金額', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'FinishAmount';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'爆率', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'Rate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否進行中 0無 1正在放分', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'Status';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否完成 0未完成 1完成', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'Finish';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'完成時間', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'FinishTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'放分時間', @level0type = N'SCHEMA', @level0name = N'Game', @level1type = N'TABLE', @level1name = N'SuperTool_Fish_SystemReward', @level2type = N'COLUMN', @level2name = N'CreateTime';

