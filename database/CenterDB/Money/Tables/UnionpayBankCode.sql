CREATE TABLE [Money].[UnionpayBankCode] (
    [Code]     VARCHAR (5)  NOT NULL,
    [BankName] VARCHAR (50) NOT NULL,
    [Show]     BIT          CONSTRAINT [DF_UnionpayBankCode_Show] DEFAULT ((1)) NOT NULL,
    [BankID]   VARCHAR (50) NULL,
    CONSTRAINT [PK_UnionpayBankCode] PRIMARY KEY CLUSTERED ([Code] ASC)
);

