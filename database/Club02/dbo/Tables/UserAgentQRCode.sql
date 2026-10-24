CREATE TABLE [dbo].[UserAgentQRCode] (
    [UserID]     INT           NOT NULL,
    [QRCode]     VARCHAR (20)  NOT NULL,
    [QRURL]      VARCHAR (250) NULL,
    [Status]     TINYINT       NOT NULL,
    [DomainID]   INT           NULL,
    [Mode1]      TINYINT       NULL,
    [Mode2]      TINYINT       NULL,
    [Mode3]      TINYINT       NULL,
    [Mode4]      TINYINT       NULL,
    [Mode5]      TINYINT       NULL,
    [CreateTime] DATETIME      NULL,
    [NickName]   VARCHAR (32)  NULL,
    [Password]   VARCHAR (32)  NULL,
    [Encrypt]    VARCHAR (10)  NULL,
    [Mode6]      TINYINT       NULL,
    CONSTRAINT [PK_UserQRCode] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

