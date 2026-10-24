CREATE TABLE [Event].[Ticket] (
    [ID]         INT      IDENTITY (1, 1) NOT NULL,
    [ContestID]  INT      NOT NULL,
    [UserID]     INT      NOT NULL,
    [CreateTime] DATETIME NOT NULL,
    [UpdateTime] DATETIME NULL,
    [Type]       TINYINT  NOT NULL,
    CONSTRAINT [PK_Ticket] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_EventTicket_UserID]
    ON [Event].[Ticket]([ContestID] ASC, [UserID] ASC, [CreateTime] ASC, [UpdateTime] ASC, [Type] ASC)
    INCLUDE([ID]);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'領取/購買時間', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Ticket', @level2type = N'COLUMN', @level2name = N'CreateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'使用時間', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Ticket', @level2type = N'COLUMN', @level2name = N'UpdateTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'贈送1/自購2', @level0type = N'SCHEMA', @level0name = N'Event', @level1type = N'TABLE', @level1name = N'Ticket', @level2type = N'COLUMN', @level2name = N'Type';

