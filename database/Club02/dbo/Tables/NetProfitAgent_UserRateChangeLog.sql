CREATE TABLE [dbo].[NetProfitAgent_UserRateChangeLog] (
    [ID]          BIGINT   IDENTITY (1, 1) NOT NULL,
    [UserID]      INT      NOT NULL,
    [Rate]        INT      NOT NULL,
    [Status]      TINYINT  NOT NULL,
    [CreateTime]  DATETIME NULL,
    [UpdateUser]  INT      NULL,
    [UpdateAdmin] INT      NULL,
    CONSTRAINT [PK_NetProfitAgent_UserRateChangeLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

