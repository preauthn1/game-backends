CREATE TABLE [Users].[UserRegLog] (
    [LogID]      BIGINT       IDENTITY (1, 1) NOT NULL,
    [UserID]     INT          NOT NULL,
    [HDType]     TINYINT      NOT NULL,
    [CreateTime] DATETIME     NOT NULL,
    [PlatformID] INT          NULL,
    [AreaID]     INT          NULL,
    [HDCode]     VARCHAR (64) NULL,
    [IPAddress]  VARCHAR (40) NULL,
    CONSTRAINT [PK_UserRegLog] PRIMARY KEY CLUSTERED ([LogID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_UserRegLog_CreateTime]
    ON [Users].[UserRegLog]([UserID] ASC, [CreateTime] ASC);

