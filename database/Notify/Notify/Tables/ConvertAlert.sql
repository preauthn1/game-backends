CREATE TABLE [Notify].[ConvertAlert] (
    [Id]         INT          IDENTITY (1, 1) NOT NULL,
    [UserID]     INT          NOT NULL,
    [UserName]   VARCHAR (50) NOT NULL,
    [WinLost]    BIGINT       NOT NULL,
    [Status]     TINYINT      CONSTRAINT [DF_ConvertAlert_Status] DEFAULT ((1)) NOT NULL,
    [CreateTime] DATETIME     CONSTRAINT [DF_ConvertAlert_CreateTime] DEFAULT (getdate()) NOT NULL,
    [Multiple]   FLOAT (53)   NULL,
    CONSTRAINT [PK_ConvertAlert] PRIMARY KEY CLUSTERED ([Id] ASC)
);

