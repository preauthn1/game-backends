CREATE TABLE [Game].[SuperTool_FishItem] (
    [FishId]    INT           NOT NULL,
    [Name]      NVARCHAR (50) NULL,
    [ModelName] VARCHAR (100) NULL,
    CONSTRAINT [PK_SuperTool_FishItem] PRIMARY KEY CLUSTERED ([FishId] ASC)
);

