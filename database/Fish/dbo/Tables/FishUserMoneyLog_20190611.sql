CREATE TABLE [dbo].[FishUserMoneyLog_20190611] (
    [SeqID]        INT           IDENTITY (1, 1) NOT NULL,
    [UserID]       INT           NULL,
    [ServerID]     INT           NULL,
    [Exp]          INT           NULL,
    [OnlineTime]   INT           NULL,
    [WinMoney]     BIGINT        NULL,
    [LoseMoney]    BIGINT        NULL,
    [ShotNum]      INT           NULL,
    [MissMoneyLib] INT           NULL,
    [CreateDate]   DATETIME2 (7) NULL
);

