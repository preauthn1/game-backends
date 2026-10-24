CREATE TABLE [Stat].[GameAmountLog] (
    [StatTime]        DATETIME NOT NULL,
    [PlatformID]      INT      NOT NULL,
    [AreaID]          INT      NOT NULL,
    [GameID]          INT      NOT NULL,
    [UserAmount]      BIGINT   CONSTRAINT [DF_GameAmountLog_UserAmount] DEFAULT ((0)) NOT NULL,
    [SystemAmount]    BIGINT   CONSTRAINT [DF_GameAmountLog_SystemAmount] DEFAULT ((0)) NOT NULL,
    [UserTaxAmount]   BIGINT   NOT NULL,
    [SystemTaxAmount] BIGINT   NOT NULL
);


GO
CREATE NONCLUSTERED INDEX [IDX_GameAmountLog_StatTime]
    ON [Stat].[GameAmountLog]([StatTime] ASC);

