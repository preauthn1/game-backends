CREATE TABLE [Pay].[OfflineAccount] (
    [Id]          INT           IDENTITY (1, 1) NOT NULL,
    [ConvertType] TINYINT       NOT NULL,
    [RealName]    VARCHAR (64)  NOT NULL,
    [Account]     VARCHAR (64)  NOT NULL,
    [BankName]    VARCHAR (50)  NULL,
    [Link]        VARCHAR (512) NULL,
    [MinGrade]    INT           NOT NULL,
    [MaxGrade]    INT           NOT NULL,
    [MinValue]    BIGINT        NOT NULL,
    [MaxValue]    BIGINT        NOT NULL,
    [Memo]        VARCHAR (200) NULL,
    [Status]      TINYINT       NOT NULL,
    [Company]     INT           NOT NULL
);

