CREATE TABLE [Stat].[ActiveUserKeepLog] (
    [Id]          INT        IDENTITY (1, 1) NOT NULL,
    [YYYYMMDD]    INT        NOT NULL,
    [ActiveUsers] INT        NULL,
    [Day1Keep]    FLOAT (53) NULL,
    [Day3Keep]    FLOAT (53) NULL,
    [Day5Keep]    FLOAT (53) NULL,
    [Day7Keep]    FLOAT (53) NULL,
    [Day14Keep]   FLOAT (53) NULL,
    [Day30Keep]   FLOAT (53) NULL,
    [Day3Payrate] FLOAT (53) NULL,
    [Day5Payrate] FLOAT (53) NULL,
    [Day7Payrate] FLOAT (53) NULL,
    CONSTRAINT [PK_ActiveUserKeepLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);

