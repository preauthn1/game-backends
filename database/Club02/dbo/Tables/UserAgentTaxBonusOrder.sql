CREATE TABLE [dbo].[UserAgentTaxBonusOrder] (
    [ID]            BIGINT        NOT NULL,
    [RelationTaxID] BIGINT        NOT NULL,
    [LogDate]       INT           NOT NULL,
    [LogHour]       INT           NOT NULL,
    [UserID]        INT           NOT NULL,
    [PlatformID]    INT           NOT NULL,
    [AreaID]        INT           NOT NULL,
    [Amount]        BIGINT        NOT NULL,
    [Status]        TINYINT       NOT NULL,
    [Memo]          VARCHAR (250) NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([ID] ASC)
);

