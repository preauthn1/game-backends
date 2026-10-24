CREATE TABLE [dbo].[Split_UserMission] (
    [UserID]           INT      NOT NULL,
    [MobileBind]       INT      NULL,
    [MobileBindDate]   DATETIME NULL,
    [AlipayBind]       INT      NULL,
    [AlipayBindDate]   DATETIME NULL,
    [UnionPayBind]     INT      NULL,
    [UnionPayBindDate] DATETIME NULL,
    [ChargeBind]       INT      NULL,
    [ChargeBindDate]   DATETIME NULL,
    [ConvertBind]      INT      NULL,
    [ConvertBindDate]  DATETIME NULL,
    CONSTRAINT [PK_Split_UserMission] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

