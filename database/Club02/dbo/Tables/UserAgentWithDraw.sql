CREATE TABLE [dbo].[UserAgentWithDraw] (
    [ID]             BIGINT        IDENTITY (1, 1) NOT NULL,
    [LogDate]        DATETIME      NOT NULL,
    [UserID]         INT           NOT NULL,
    [WithdrawAmount] BIGINT        NOT NULL,
    [Status]         TINYINT       CONSTRAINT [DF_UserAgent_WithDraw_Status] DEFAULT ((0)) NOT NULL,
    [Memo]           VARCHAR (MAX) NULL,
    [UpdateTime]     DATETIME      NULL,
    CONSTRAINT [PK_UserAgentWithDraw] PRIMARY KEY CLUSTERED ([ID] ASC)
);

