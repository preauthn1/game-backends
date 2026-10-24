CREATE TABLE [dbo].[Schedule_UpdateUserStageRate] (
    [UserID]       INT     NOT NULL,
    [Stage1]       TINYINT NOT NULL,
    [Stage2]       TINYINT NOT NULL,
    [Stage3]       TINYINT NOT NULL,
    [UpdateStatus] TINYINT NOT NULL,
    CONSTRAINT [PK_Schedule_UpdateUserStageRate] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

