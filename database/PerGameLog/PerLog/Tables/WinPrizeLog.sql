CREATE TABLE [PerLog].[WinPrizeLog] (
    [Id]         INT          IDENTITY (1, 1) NOT NULL,
    [AdminUser]  VARCHAR (20) NOT NULL,
    [GameID]     INT          NOT NULL,
    [ServerID]   INT          NOT NULL,
    [TableNo]    INT          NOT NULL,
    [PrizeType]  VARCHAR (50) NOT NULL,
    [LogID]      BIGINT       CONSTRAINT [DF_WinPrizeLog_LogID] DEFAULT ((0)) NOT NULL,
    [UpdateTime] DATETIME     NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_WinPrizeLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_WinPrizeLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_WinPrizeLog]
    ON [PerLog].[WinPrizeLog]([GameID] ASC, [ServerID] ASC, [TableNo] ASC);

