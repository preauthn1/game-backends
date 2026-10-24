CREATE TABLE [Pay].[PayType] (
    [GWID]         INT           NOT NULL,
    [TypeID]       INT           NOT NULL,
    [TypeName]     VARCHAR (32)  NOT NULL,
    [TradeCode]    VARCHAR (20)  CONSTRAINT [DF_PayType_TradeCode] DEFAULT ((0)) NOT NULL,
    [SourceTypeID] INT           NOT NULL,
    [Scale]        INT           NOT NULL,
    [Status]       TINYINT       NOT NULL,
    [AmountSort]   VARCHAR (512) NULL,
    [IsRange]      BIT           NULL,
    [MinValue]     BIGINT        NULL,
    [MaxValue]     BIGINT        NULL,
    CONSTRAINT [PK_PayType] PRIMARY KEY CLUSTERED ([GWID] ASC, [TypeID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'0 停用 1启用 2不显示在后台', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'PayType', @level2type = N'COLUMN', @level2name = N'Status';

