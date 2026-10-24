CREATE TABLE [dbo].[NetProfitAgent_SystemRate] (
    [PlatformID]       INT     NOT NULL,
    [Rate]             INT     NOT NULL,
    [WithDrawDayLimit] TINYINT NOT NULL,
    [StageLevel]       TINYINT NOT NULL,
    CONSTRAINT [PK_NetProfitAgent_SystemRate_1] PRIMARY KEY CLUSTERED ([PlatformID] ASC)
);

