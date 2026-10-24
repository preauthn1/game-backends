CREATE TABLE [Pay].[RedirectData] (
    [Id]         INT           IDENTITY (1, 1) NOT NULL,
    [GWID]       INT           NOT NULL,
    [FormData]   VARCHAR (MAX) NOT NULL,
    [CreateTime] DATETIME      NOT NULL,
    [ExpireTime] DATETIME      NOT NULL,
    [GUID]       VARCHAR (32)  NOT NULL,
    CONSTRAINT [PK_RedirectData] PRIMARY KEY CLUSTERED ([Id] ASC)
);



