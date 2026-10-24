CREATE TABLE [Pay].[ConvertBankCode] (
    [GWID]      INT          NOT NULL,
    [Code]      VARCHAR (5)  NOT NULL,
    [TradeCode] VARCHAR (20) NOT NULL,
    [Status]    TINYINT      NOT NULL,
    CONSTRAINT [PK_ConvertBankCode] PRIMARY KEY CLUSTERED ([GWID] ASC, [Code] ASC)
);

