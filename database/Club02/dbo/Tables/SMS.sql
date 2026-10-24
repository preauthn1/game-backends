CREATE TABLE [dbo].[SMS] (
    [VerifyCode] VARCHAR (10) NOT NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_SMS_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SMS] PRIMARY KEY CLUSTERED ([VerifyCode] ASC)
);

