CREATE TABLE [Log].[PrWeb_Pay_FinishedOrder] (
    [Id]           INT           IDENTITY (1, 1) NOT NULL,
    [ErrorLogID]   INT           NULL,
    [intOrderID]   INT           NULL,
    [lngAmount]    BIGINT        NULL,
    [chvIPAddress] VARCHAR (15)  NULL,
    [chvTransID]   VARCHAR (32)  NULL,
    [chvTransData] VARCHAR (MAX) NULL,
    [chvErrMsg]    VARCHAR (128) NULL,
    [WriteTime]    DATETIME      NOT NULL
);

