CREATE TABLE [Admin].[AdminLog] (
    [Id]        INT            IDENTITY (1, 1) NOT NULL,
    [UserID]    INT            NOT NULL,
    [Memo]      VARCHAR (512)  NOT NULL,
    [Url]       VARCHAR (1024) NOT NULL,
    [Data]      VARCHAR (MAX)  NULL,
    [IPAddress] VARCHAR (15)   NOT NULL,
    [WriteTime] DATETIME       NOT NULL,
    CONSTRAINT [PK_AdminLog] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_AdminLog_UserID]
    ON [Admin].[AdminLog]([UserID] ASC);


GO


-- =============================================
-- Author:  Jason
-- Create date: 2018/06/13
-- Description:	依據客服活動紀錄更新最後登入時間
-- =============================================
CREATE TRIGGER [Admin].[TGR_AdminLog]
ON [Admin].[AdminLog] AFTER INSERT
AS
BEGIN
	DECLARE @NowTime	DATETIME = GETDATE()

	DECLARE @UserID		INT

	SELECT @UserID = INSERTED.UserID FROM INSERTED

    UPDATE [Admin].[Users]
	SET [LastLogonTime] = @NowTime
	WHERE
		[UserID] = @UserID
END




