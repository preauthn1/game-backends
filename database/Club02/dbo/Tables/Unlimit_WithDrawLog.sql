CREATE TABLE [dbo].[Unlimit_WithDrawLog] (
    [ID]                 BIGINT        IDENTITY (1, 1) NOT NULL,
    [UserID]             INT           NOT NULL,
    [WithDrawAmount]     BIGINT        NOT NULL,
    [WithdrawType]       INT           NOT NULL,
    [Type]               INT           NOT NULL,
    [Status]             TINYINT       NOT NULL,
    [CreateTime]         DATETIME      NOT NULL,
    [TaxAmount]          BIGINT        NULL,
    [PoolAmount]         BIGINT        NULL,
    [PoolWithDrawAmount] BIGINT        NULL,
    [PoolLastUpdateTime] DATETIME      NULL,
    [Updatetime]         DATETIME      NULL,
    [UpdateUser]         VARCHAR (50)  NULL,
    [Memo]               VARCHAR (100) NULL,
    [RealName]           VARCHAR (64)  NULL,
    [AlipayAccount]      VARCHAR (64)  NULL,
    [UnionpayBank]       VARCHAR (5)   NULL,
    [UnionpayAccount]    VARCHAR (64)  NULL,
    [UnionpayRealName]   VARCHAR (64)  NULL,
    [ipaddress]          VARCHAR (15)  NULL,
    CONSTRAINT [PK_Unlimit_WithDrawLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

