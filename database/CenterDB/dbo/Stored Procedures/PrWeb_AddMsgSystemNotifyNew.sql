-- =============================================
-- Author:  Rory
-- Create date: 2019/07/12
-- Description:	后台 发送系统讯息(for 特定平台 渠道或特定USERID发送)
-- @intPlatformID 未指定用戶則不可以為0, 不支持全系統平台發送
-- @intAreaID 為0 則發送該平台下所有渠道用戶
-- @chvToUserID 可為多個, 當指定用戶時, 需要檢查管理權限是否擁有該用戶平台權限.
-- 三者为NULL时直接RETURN
-- 2019/07/18	Eddie	Set @intAreaID = ISNULL(@intAreaID,0)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddMsgSystemNotifyNew]
(
	@intAdminID		int,
	@intPlatformID	int = 0,
	@intAreaID		int = 0,
	@chvTitle		NVARCHAR(255),
	@chvContent		NVARCHAR(MAX),
	@tnyMsgType		TINYINT,
	@chvToUserID	VARCHAR(MAX) = NULL,
	@tnySort		TINYINT,
	@intRtnCode		INT				OUTPUT,
	@chvErrMsg		VARCHAR(100)	OUTPUT
)
AS
BEGIN
	
	SET NOCOUNT ON

	Set @intAreaID = ISNULL(@intAreaID,0)

	--declare @chvToUserID NVARCHAR(MAX) = ''
	--declare @intRtnCode		INT		
	--declare @chvErrMsg		VARCHAR(100)
	DECLARE @NowTime DATETIME = GETDATE()
	DECLARE @NoticeMsgSeq	BIGINT

	DECLARE @Temp_PlatformArea	TABLE (PlatformID INT,AreaID INT)
	DECLARE @Temp_ToUser	TABLE (UserID INT)
	DECLARE @ToUserCount INT
	SET @intRtnCode = -1
	-- 獲取管理員平台權限表
	SELECT * INTO #Temp_AccessRights FROM dbo.Fn_GetAdminAccessRights(@intAdminID)

	SELECT CAST(Value AS INT) AS UserID INTO #Temp_UserIDs 
		FROM [dbo].[fn_Split2](@chvToUserID,',') WHERE Value <> ''
	SET @ToUserCount = @@ROWCOUNT

	--如果為指定玩家則需要檢查是否有權限操作
	IF (@ToUserCount > 0)
	BEGIN
		-- 將要發送的用戶清單保存在暫存表內
		INSERT INTO @Temp_ToUser 
			SELECT UserID FROM Users.UserInfo WHERE UserID IN ( 
			SELECT UserID FROM #Temp_UserIDs
		) AND AreaID IN ( SELECT AreaID FROM #Temp_AccessRights )
	END
	ELSE
	BEGIN
		-- 不允許全平台發送, 沒指定玩家且平台為 0 或找不到平台號, 則返回錯誤
		IF ( ( @ToUserCount = 0 AND @intPlatformID = 0) OR NOT EXISTS ( SELECT 1 FROM Area.Platform WHERE PlatformID = @intPlatformID ) )
		BEGIN
			SET @chvErrMsg = '参数错误'
			SET @intRtnCode = -1
			RETURN
		END

		-- 未指定玩家則透過渠道平台發送, 查找平台下所有渠道或指定渠道號, 並檢查是否有權限
		IF (@intAreaID = 0)
		BEGIN
			INSERT INTO @Temp_PlatformArea
			SELECT PlatformID, AreaID FROM Area.Area WHERE PlatformID = @intPlatformID AND AreaID IN (
				SELECT AreaID FROM #Temp_AccessRights
			)
		END
		ELSE
		BEGIN
			INSERT INTO @Temp_PlatformArea
			SELECT PlatformID, AreaID FROM Area.Area WHERE PlatformID = @intPlatformID AND AreaID = @intAreaID AND AreaID IN (
				SELECT AreaID FROM #Temp_AccessRights
			)
		END

		SELECT 1 FROM @Temp_PlatformArea
		-- 檢查是否有可用的平台渠道號
		IF (@@ROWCOUNT = 0)
		BEGIN
			SET @chvErrMsg = '平台渠道號錯誤'
			SET @intRtnCode = -1
			RETURN
		END
		-- 將要發送的用戶清單保存在暫存表內(機器人不發送)
		INSERT INTO @Temp_ToUser 
		SELECT UserID FROM Users.UserInfo WHERE UserType<>2 and AreaID IN (
			SELECT AreaID FROM @Temp_PlatformArea
		)
	END

	DROP TABLE #Temp_UserIDs

	SELECT 1 FROM @Temp_ToUser
	IF (@@ROWCOUNT = 0)
	BEGIN
		SET @chvErrMsg = '沒有可發送的用戶清單'
		SET @intRtnCode = -1
		RETURN
	END

	SET @NoticeMsgSeq = CAST(RIGHT(REPLACE(REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR(23),@NowTime,121),'-',''),' ',''),':',''),'.',''),15) + '0' AS BIGINT)

	-- 批量發送記錄
	INSERT INTO [Message].[MessageNotice]
			([NoticeMsgSeq],[PlatformID],[AreaID],[Title],[MsgContent],[CreateTime],[Type],[Sort])
		SELECT	@NoticeMsgSeq,
				PlatformID,
				AreaID,
				@chvTitle,
				@chvContent,
				@NowTime,
				@tnyMsgType,
				@tnySort
		FROM @Temp_PlatformArea

	IF @@ERROR <> 0
	BEGIN
		SET @chvErrMsg = '添加失败'
		SET @intRtnCode = -1
	END
	ELSE
	BEGIN
		SET @chvErrMsg = '添加成功'
		SET @intRtnCode = 1
	END

	-- 批量發送執行
	DECLARE @Temp_MsgID TABLE (MsgID BIGINT)

	INSERT INTO [Message].[MessageList]
		([Title],[MsgContent],[CreateTime],[Type],[Sort],[MsgSeq])
	OUTPUT INSERTED.MsgID INTO @Temp_MsgID
	VALUES (@chvTitle,@chvContent,@NowTime,@tnyMsgType,@tnySort,@NoticeMsgSeq)

	IF @@ERROR <> 0
	BEGIN
		SET @chvErrMsg = '添加失败'
		SET @intRtnCode = -1
	END
	ELSE
	BEGIN
		SET @chvErrMsg = '添加成功'
		SET @intRtnCode = 1
	END

	--个别写入通知
	DECLARE @MsgID BIGINT

	SELECT	@MsgID = MsgID FROM @Temp_MsgID

	INSERT INTO [Message].[ReceiveUser]
		([MsgID],[UserID],[Status],[MsgSeq])
	SELECT	@MsgID,
			UserID,
			0,
			@NoticeMsgSeq
	FROM @Temp_ToUser

	IF @@ERROR <> 0
	BEGIN
		SET @chvErrMsg = '添加失败'
		SET @intRtnCode = -1
	END
	ELSE
	BEGIN
		SET @chvErrMsg = '添加成功'
		SET @intRtnCode = 1
	END
	
	-- 通知客戶端
	DECLARE @countTemp INT, @NotifyUserID INT --用來計算還剩幾筆資料

	SET @countTemp = @@ROWCOUNT
	WHILE(@countTemp >0)
	BEGIN
		SET ROWCOUNT  1
		SELECT @NotifyUserID = UserID FROM @Temp_ToUser
		EXEC NOTIFYLINKEDSERVER.[Notify].[dbo].[PrNs_SendSysMail]
			@intUserID = @NotifyUserID,
			@tnyTypeID = 1,
			@bintParam = 0,
			@tnyStatus = 0,
			@dtCreateTime = @NowTime,
			@dtLastPostTime = @NowTime
		DELETE FROM @Temp_ToUser
		SELECT @countTemp = count(0) FROM @Temp_ToUser
	END
	RETURN
END

GO


