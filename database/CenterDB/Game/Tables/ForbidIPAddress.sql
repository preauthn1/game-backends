CREATE TABLE [Game].[ForbidIPAddress] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [IPAddress]  VARCHAR (15)  NOT NULL,
    [Memo]       VARCHAR (128) NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    [ExpireTime] DATETIME      NOT NULL,
    [IPAddress2] VARCHAR (40)  NULL,
    [Min]        BIGINT        NULL,
    [Max]        BIGINT        NULL,
    CONSTRAINT [PK_ForbidIPAddress] PRIMARY KEY CLUSTERED ([Id] ASC) WITH (FILLFACTOR = 80)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_ForbidIPAddress_IPAddress]
    ON [Game].[ForbidIPAddress]([IPAddress] ASC);

