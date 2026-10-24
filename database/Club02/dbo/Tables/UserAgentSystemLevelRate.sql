CREATE TABLE [dbo].[UserAgentSystemLevelRate] (
    [Level]     INT    NOT NULL,
    [Stage1]    INT    NOT NULL,
    [Stage2]    INT    NOT NULL,
    [Stage3]    INT    NOT NULL,
    [TaxAmount] BIGINT NOT NULL,
    CONSTRAINT [PK_UserAgentSystemLevel] PRIMARY KEY CLUSTERED ([Level] ASC)
);

