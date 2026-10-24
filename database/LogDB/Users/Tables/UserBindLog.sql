CREATE TABLE [Users].[UserBindLog] (
    [LogID]      INT      IDENTITY (1, 1) NOT NULL,
    [UserID]     INT      NOT NULL,
    [HDType]     TINYINT  NOT NULL,
    [PlatformID] INT      NOT NULL,
    [AreaID]     INT      NOT NULL,
    [CreateTime] DATETIME CONSTRAINT [DF_UserBindLog_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_UserBindLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_UserBindLog_CreateTime]
    ON [Users].[UserBindLog]([CreateTime] ASC);

