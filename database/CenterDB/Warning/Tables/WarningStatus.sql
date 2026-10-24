CREATE TABLE [Warning].[WarningStatus] (
    [UserID]        INT     NOT NULL,
    [WarningStatus] TINYINT NOT NULL,
    CONSTRAINT [PK_WarningStatus] PRIMARY KEY CLUSTERED ([UserID] ASC) WITH (FILLFACTOR = 80)
);

