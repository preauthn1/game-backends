CREATE TABLE [Stat].[ConvertLog] (
    [Id]            INT      IDENTITY (1, 1) NOT NULL,
    [Amount]        BIGINT   NOT NULL,
    [TaxAmount]     BIGINT   CONSTRAINT [DF_ConvertLog_TaxAmount] DEFAULT ((0)) NOT NULL,
    [RealTaxAmount] BIGINT   CONSTRAINT [DF_ConvertLog_RealAmount] DEFAULT ((0)) NOT NULL,
    [StatTime]      DATETIME NOT NULL,
    CONSTRAINT [PK_ConvertLog_Id] PRIMARY KEY NONCLUSTERED ([Id] ASC)
);


GO
CREATE CLUSTERED INDEX [IUX_ConvertLog_StatTime]
    ON [Stat].[ConvertLog]([StatTime] ASC);

