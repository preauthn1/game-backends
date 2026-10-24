CREATE TABLE [Game].[SuperTool_Fish_Data] (
    [FlagID]       INT    NOT NULL,
    [FishID]       INT    NOT NULL,
    [Amount]       BIGINT CONSTRAINT [DF_SuperTool_Fish_Data_Amount] DEFAULT ((0)) NOT NULL,
    [RewardAmount] BIGINT CONSTRAINT [DF_SuperTool_Fish_Data_RewardAmount] DEFAULT ((0)) NOT NULL,
    [YYYYMMDD]     INT    CONSTRAINT [DF_SuperTool_Fish_Data_YYYYMMDD] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_SuperTool_Fish_Data] PRIMARY KEY CLUSTERED ([FlagID] ASC, [FishID] ASC, [YYYYMMDD] ASC)
);

