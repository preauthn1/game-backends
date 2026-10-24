CREATE TABLE [dbo].[Split_UserBonusLog] (
    [ID]          BIGINT   IDENTITY (1, 1) NOT NULL,
    [UserID]      INT      NOT NULL,
    [FromUserID]  INT      NOT NULL,
    [BonusAmount] BIGINT   NOT NULL,
    [CreateTime]  DATETIME NOT NULL,
    CONSTRAINT [PK_Split_UserBonusLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);

