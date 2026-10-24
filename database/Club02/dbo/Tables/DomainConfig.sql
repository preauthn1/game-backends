CREATE TABLE [dbo].[DomainConfig] (
    [ID]        INT          IDENTITY (1, 1) NOT NULL,
    [Domain]    VARCHAR (50) NOT NULL,
    [Count]     INT          CONSTRAINT [DF_DomainConifg_Count] DEFAULT ((0)) NOT NULL,
    [Flag]      TINYINT      CONSTRAINT [DF_DomainConfig_Flag_1] DEFAULT ((0)) NOT NULL,
    [Available] TINYINT      CONSTRAINT [DF_DomainConfig_Available_1] DEFAULT ((1)) NOT NULL,
    CONSTRAINT [PK_DomainConifg] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否可分配 1是 0否', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'DomainConfig', @level2type = N'COLUMN', @level2name = N'Available';

