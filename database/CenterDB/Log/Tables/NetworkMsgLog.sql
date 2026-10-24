CREATE TABLE [Log].[NetworkMsgLog] (
    [Id]       INT IDENTITY (1, 1) NOT NULL,
    [ServerID] INT NOT NULL,
    [MsgID]    INT NOT NULL,
    CONSTRAINT [PK_NetworkMsgLog_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_NetworkMsgLog_ServerID_MsgID]
    ON [Log].[NetworkMsgLog]([ServerID] ASC, [MsgID] ASC);

