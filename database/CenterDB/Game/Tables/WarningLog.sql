CREATE TABLE [Game].[WarningLog] (
    [LogID]     INT           IDENTITY (1, 1) NOT NULL,
    [Memo]      VARCHAR (MAX) NOT NULL,
    [WriteTime] DATETIME      NOT NULL,
    CONSTRAINT [PK_WarningLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

