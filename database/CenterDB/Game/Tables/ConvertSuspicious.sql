CREATE TABLE [Game].[ConvertSuspicious] (
    [Id]         INT          IDENTITY (1, 1) NOT NULL,
    [Name]       VARCHAR (50) NULL,
    [AccountID]  VARCHAR (50) NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_ConvertSuspiciousAccount_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ConvertSuspicious] PRIMARY KEY CLUSTERED ([Id] ASC)
);

