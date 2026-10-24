CREATE TABLE [Game].[ForbidUser] (
    [UserID]     INT           NOT NULL,
    [CashAmount] BIGINT        NOT NULL,
    [BankAmount] BIGINT        NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    [Memo]       VARCHAR (128) NOT NULL,
    CONSTRAINT [PK_ForbidUser] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

