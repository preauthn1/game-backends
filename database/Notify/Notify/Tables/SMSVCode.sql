CREATE TABLE [Notify].[SMSVCode] (
    [CodeID]       INT            IDENTITY (1, 1) NOT NULL,
    [Tel]          VARCHAR (15)   NOT NULL,
    [VCode]        VARCHAR (32)   NOT NULL,
    [Status]       TINYINT        NOT NULL,
    [CreateTime]   DATETIME       NOT NULL,
    [LastSendTime] DATETIME       NOT NULL,
    [IPAddress]    VARCHAR (40)   NULL,
    [CodeType]     TINYINT        DEFAULT ((0)) NULL,
    [UserID]       INT            NULL,
    [ExpireTime]   DATETIME       NULL,
    [UpdateTime]   DATETIME       NULL,
    [SendResult]   NVARCHAR (512) NULL,
    [Vender]       VARCHAR (20)   NULL,
    CONSTRAINT [PK_SMSVCode] PRIMARY KEY CLUSTERED ([CodeID] ASC)
);

