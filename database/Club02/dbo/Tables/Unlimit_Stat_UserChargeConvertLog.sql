CREATE TABLE [dbo].[Unlimit_Stat_UserChargeConvertLog] (
    [LogDate]       DATETIME NOT NULL,
    [UserID]        INT      NOT NULL,
    [ChargeAmount]  BIGINT   CONSTRAINT [DF_Unlimit_Stat_UserChargeConvertLog_ChargeAmount] DEFAULT ((0)) NULL,
    [ConvertAmount] BIGINT   CONSTRAINT [DF_Unlimit_Stat_UserChargeConvertLog_ConvertAmount] DEFAULT ((0)) NULL,
    CONSTRAINT [PK_Unlimit_Stat_UserChargeConvertLog] PRIMARY KEY CLUSTERED ([LogDate] ASC, [UserID] ASC)
);

