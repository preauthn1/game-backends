CREATE TABLE [Message].[MessageNotice] (
    [NoticeMsgSeq] BIGINT         NOT NULL,
    [PlatformID]   INT            NOT NULL,
    [AreaID]       INT            NOT NULL,
    [Title]        NVARCHAR (100) NOT NULL,
    [MsgContent]   NVARCHAR (MAX) NOT NULL,
    [Type]         TINYINT        NOT NULL,
    [Sort]         INT            NOT NULL,
    [CreateTime]   DATETIME       NOT NULL,
    CONSTRAINT [PK_MessageNotice_1] PRIMARY KEY CLUSTERED ([NoticeMsgSeq] ASC, [PlatformID] ASC, [AreaID] ASC)
);

