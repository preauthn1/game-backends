CREATE TABLE [Game].[Fish_SystemPool] (
    [FlagID] INT    NOT NULL,
    [Amount] BIGINT CONSTRAINT [DF_Fish_SystemPool_Amount] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Fish_SystemPool] PRIMARY KEY CLUSTERED ([FlagID] ASC)
);

