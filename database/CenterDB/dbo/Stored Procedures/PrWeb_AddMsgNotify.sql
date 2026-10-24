-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<邮件通知>
--	2018/03/21	Jason		調整Notify資料表寫入改為呼叫SP執行
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddMsgNotify]
@intMsgID INT
AS
BEGIN

	SET NOCOUNT ON
	DECLARE @dtStatTime	DATETIME = GETDATE()
	DECLARE @intUserID	INT
	DECLARE @tnyTypeID	TINYINT = 1
	DECLARE	@lngParam	BIGINT = 0
	DECLARE	@tnyStatus	TINYINT = 0

	SELECT	@intUserID = UserID
	FROM Message.ReceiveUser WITH (NOLOCK)
	WHERE MsgID = @intMsgID

	IF @intUserID IS NOT NULL
	BEGIN
		--INSERT INTO NOTIFYLINKEDSERVER.Notify.Notify.Attr
		--	( UserID ,
		--	  TypeID ,
		--	  Param ,
		--	  Status ,
		--	  CreateTime ,
		--	  LastPostTime
		--	)
		--SELECT @intUserID,
		--	1,
		--	0,
		--	0,
		--	GETDATE(),
		--	GETDATE()

		EXEC [NOTIFYLINKEDSERVER].[Notify].[dbo].[PrNs_Attr_Add]
				@intUserID = @intUserID,
				@tnyTypeID = @tnyTypeID,
				@lngParam = @lngParam,
				@tnyStatus = @tnyStatus,
				@dtStatTime = @dtStatTime

	END

	RETURN 1

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddMsgNotify] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddMsgNotify] TO [xbuser]
    AS [dbo];

