CREATE TABLE [Pay].[OfflineOrder] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [UserID]         INT           NOT NULL,
    [Payer]          VARCHAR (64)  NOT NULL,
    [Amount]         BIGINT        NOT NULL,
    [ConvertType]    TINYINT       NOT NULL,
    [RealName]       VARCHAR (64)  NOT NULL,
    [Account]        VARCHAR (64)  NOT NULL,
    [BankName]       VARCHAR (50)  NULL,
    [Memo]           VARCHAR (200) NULL,
    [Operator]       VARCHAR (16)  NULL,
    [OrderID]        INT           NOT NULL,
    [PlatformID]     INT           NOT NULL,
    [AreaID]         INT           NOT NULL,
    [Status]         TINYINT       NOT NULL,
    [CreateTime]     DATETIME      NOT NULL,
    [LastUpdateTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_OfflineOrder] PRIMARY KEY CLUSTERED ([Id] ASC)
);

