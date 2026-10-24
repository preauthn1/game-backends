-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<发送系统邮件>
--	2018/03/20	Jason		調整寫法並改為呼叫notify SP
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_SendSysMail]
@intUserID INT,						--收件人ID
@chvTitle VARCHAR(128),				--邮件标题
@chvContent VARCHAR(1024),			--邮件内容
@intSort INT,						--排序
@intMsgID INT OUTPUT				--消息ID
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @NotifyType	TINYINT = 1

	INSERT INTO Message.MessageList
		(	Title,		MsgContent,	Type,	FromUserID,	Sort,		CreateTime	)
	SELECT	@chvTitle,	@chvContent,1,		NULL,		@intSort,	GETDATE()
	
	SET @intMsgID = @@IDENTITY

	INSERT INTO Message.ReceiveUser
		(	MsgID,		UserID,		Status,	ReadTime )
	SELECT	@intMsgID,	@intUserID, 0,		NULL
	

	/*  --邮件通知  */
	----------------------------------------------------------------------
	--INSERT INTO NOTIFYLINKEDSERVER.Notify.Notify.Attr
	--		( UserID ,
	--		  TypeID ,
	--		  Param ,
	--		  Status ,
	--		  CreateTime ,
	--		  LastPostTime
	--		)
	--SELECT @intUserID,
	--	1,
	--	0,
	--	0,
	--	GETDATE(),
	--	GETDATE()
	
	EXEC NOTIFYLINKEDSERVER.[Notify].[dbo].[PrNs_SendSysMail]
		@intUserID = @intUserID,
		@tnyTypeID = @NotifyType,
		@bintParam = 0,
		@tnyStatus = 0,
		@dtCreateTime = @NowTime,
		@dtLastPostTime = @NowTime
	----------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_SendSysMail] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_SendSysMail] TO [xbuser]
    AS [dbo];

