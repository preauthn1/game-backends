CREATE TABLE [dbo].[UserAgentInfo] (
    [UserID]          INT          NOT NULL,
    [UserLevel]       TINYINT      NOT NULL,
    [SumTaxTotal]     BIGINT       NOT NULL,
    [SumTaxBonus]     BIGINT       NOT NULL,
    [SumTaxConvert]   BIGINT       NOT NULL,
    [SumTaxPerformed] BIGINT       NOT NULL,
    [UnWithDrawTotal] BIGINT       NOT NULL,
    [CreateTime]      DATETIME     NOT NULL,
    [Status]          TINYINT      NULL,
    [TopUserID]       INT          NULL,
    [LastUpdatetime]  DATETIME     NULL,
    [PlatformID]      INT          NULL,
    [AreaID]          INT          NULL,
    [NickName]        VARCHAR (32) NULL,
    [Password]        VARCHAR (64) NULL,
    [Encrypt]         VARCHAR (8)  NULL,
    [Permission]      INT          CONSTRAINT [DF_UserAgentInfo_Permission] DEFAULT ((1)) NULL,
    CONSTRAINT [PK_UserTaxInfo] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

