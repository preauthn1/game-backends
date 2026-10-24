CREATE TABLE [dbo].[Split_UserInfo] (
    [UserID]         INT           NOT NULL,
    [StageLevel]     INT           NOT NULL,
    [CreateTime]     DATETIME      NOT NULL,
    [Relations]      VARCHAR (MAX) NULL,
    [Status]         TINYINT       NULL,
    [TopUserID]      INT           NULL,
    [UpperUserID]    INT           NULL,
    [LastUpdatetime] DATETIME      NULL,
    [PlatformID]     INT           NULL,
    [AreaID]         INT           NULL,
    [NickName]       VARCHAR (32)  NULL,
    [Password]       VARCHAR (64)  NULL,
    [Encrypt]        VARCHAR (8)   NULL,
    [Permission]     INT           CONSTRAINT [DF_Split_UserInfo_Permission] DEFAULT ((1)) NULL,
    CONSTRAINT [PK_Split_UserInfo] PRIMARY KEY CLUSTERED ([UserID] ASC)
);

