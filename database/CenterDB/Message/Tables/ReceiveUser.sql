CREATE TABLE [Message].[ReceiveUser] (
    [Id]       INT      IDENTITY (1, 1) NOT NULL,
    [MsgID]    INT      NOT NULL,
    [UserID]   INT      NOT NULL,
    [Status]   TINYINT  NOT NULL,
    [ReadTime] DATETIME NULL,
    [MsgSeq]   BIGINT   NULL,
    CONSTRAINT [PK_ReceiveUser] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_ReceiveUser_MsgID_UserID]
    ON [Message].[ReceiveUser]([MsgID] ASC, [UserID] ASC);

