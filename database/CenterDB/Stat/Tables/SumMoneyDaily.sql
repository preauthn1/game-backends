CREATE TABLE [Stat].[SumMoneyDaily] (
    [id]              INT      IDENTITY (1, 1) NOT NULL,
    [stattime]        DATETIME NULL,
    [platformid]      INT      NULL,
    [areaid]          INT      NULL,
    [totalcashamount] BIGINT   NULL,
    [totalbankamount] BIGINT   NULL,
    [usercashamount]  BIGINT   NULL,
    [userbankamount]  BIGINT   NULL,
    [totalcashdiff]   BIGINT   NULL,
    [totalbankdiff]   BIGINT   NULL,
    [usercashdiff]    BIGINT   NULL,
    [userbankdiff]    BIGINT   NULL,
    [sumin]           BIGINT   NULL,
    [sumout]          BIGINT   NULL,
    [sumindiff]       BIGINT   NULL,
    [sumoutdiff]      BIGINT   NULL,
    CONSTRAINT [PK_SumMoneyDaily] PRIMARY KEY CLUSTERED ([id] ASC)
);

