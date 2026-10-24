CREATE TABLE [Event].[Bonus] (
    [ID]         INT    IDENTITY (1, 1) NOT NULL,
    [ContestID]  INT    NOT NULL,
    [OrderReach] BIGINT NOT NULL,
    [FreeTicket] INT    NOT NULL,
    CONSTRAINT [PK_Bonus] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'場次', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Bonus', @level2type = N'COLUMN', @level2name = N'ContestID';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'充值到達金額', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Bonus', @level2type = N'COLUMN', @level2name = N'OrderReach';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'免費次數', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Bonus', @level2type = N'COLUMN', @level2name = N'FreeTicket';

