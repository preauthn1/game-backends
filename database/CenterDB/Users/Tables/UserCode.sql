CREATE TABLE [Users].[UserCode] (
    [Tel]            VARCHAR (11) NOT NULL,
    [VCode]          VARCHAR (32) NOT NULL,
    [CodeType]       TINYINT      NOT NULL,
    [CreateCodeTime] DATETIME     NOT NULL,
    CONSTRAINT [PK_UserCode_UserID_CodeType] PRIMARY KEY CLUSTERED ([Tel] ASC, [CodeType] ASC)
);

