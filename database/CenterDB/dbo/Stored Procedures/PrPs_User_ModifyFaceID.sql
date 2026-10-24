-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<修改头像>
-- 2018/03/20	Jason		調整寫法呼叫LINKEDSERVER SP執行
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_User_ModifyFaceID]
@intUserID INT,
@intFaceID INT,
@chvIPAddress varchar(15),
@chvErrMsg VARCHAR(128) OUTPUT		--提示消息
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @NotifyType	TINYINT = 3

	IF not @intFaceID between 1 and 16
	begin
		SET @chvErrMsg = '操作失败:非法头像'
		RETURN 0
	end
	
	--检查是否在游戏房间中
	IF EXISTS(SELECT 1 FROM game.GameUserList WITH(NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		SET @chvErrMsg = '操作失败:在游戏中不允许修改头像'
		RETURN 0
	END

	DECLARE @intExFaceID INT
	SELECT @intExFaceID = FaceID
	FROM Users.UserInfo
	WHERE UserID = @intUserID

	UPDATE Users.UserInfo
	SET FaceID = @intFaceID
	,Sex = case when @intFaceID<9 then 0 else 1 end	--根据头像ID确定性别
	WHERE UserID = @intUserID

	--INSERT INTO Log.UserInfoChangeLog
	--(UserID, ChangeType, OldData, NewData, IPAddress,Memo, WriteTime)
	--SELECT
	--@intUserID, 0, @intExFaceID, @intFaceID, @chvIPAddress, '客户端修改',GETDATE()

	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrPs_Add_UserInfoChangeLog]
					@intUserID = @intUserID,
					@tnyChangeType = 0,
					@chrOldData = @intExFaceID,
					@chrNewData = @intFaceID,
					@chrIPAddress = @chvIPAddress,
					@chrMemo = '客户端修改',
					@dtWriteTime = @NowTime

	--属性改变通知
	--INSERT INTO NOTIFYLINKEDSERVER.Notify.Notify.Attr
	--        ( UserID ,
	--          TypeID ,
	--          Param ,
	--          Status ,
	--          CreateTime ,
	--          LastPostTime
	--        )
	--SELECT @intUserID,
	--	3,
	--	@intFaceID,
	--	0,
	--	GETDATE(),
	--	GETDATE()

	EXEC NOTIFYLINKEDSERVER.[Notify].[dbo].[PrNs_SendSysMail]
			@intUserID = @intUserID,
			@tnyTypeID = @NotifyType,
			@bintParam = @intFaceID,
			@tnyStatus = 0,
			@dtCreateTime = @NowTime,
			@dtLastPostTime = @NowTime
	
	SET @chvErrMsg = '修改头像成功'

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ModifyFaceID] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrPs_User_ModifyFaceID] TO [xbuser]
    AS [dbo];

