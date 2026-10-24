CREATE TABLE [dbo].[LogonLog] (
    [LogID]      BIGINT           IDENTITY (1, 1) NOT NULL,
    [ID]         UNIQUEIDENTIFIER NOT NULL,
    [IPAddress]  VARCHAR (20)     NOT NULL,
    [YYYYMMDD]   INT              NOT NULL,
    [CreateTime] DATETIME         CONSTRAINT [DF_LogonLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_LogonLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

