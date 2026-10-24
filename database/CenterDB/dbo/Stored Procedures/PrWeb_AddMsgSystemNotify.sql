-- =============================================
-- Author:  jason
-- Create date: 2019/05/30
-- Description:	后台 发送系统讯息(for 特定平台 渠道或特定USERID发送)
-- 若@chvPlatformID或@chvAreaID 不为NULL则以此为发送
-- 若@chvPlatformID或@chvAreaID 为NULL则以UserID为发送
-- 三者为NULL时直接RETURN
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AddMsgSystemNotify]
(
	@chvPlatformID	VARCHAR(MAX) = NULL,
	@chvAreaID		VARCHAR(MAX) = NULL,
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

	DECLARE @NowTime DATETIME = GETDATE()
	DECLARE @NoticeMsgSeq	BIGINT

	DECLARE @Temp_PlatformArea	TABLE (PlatformID INT,AreaID INT)
	DECLARE @Temp_Platform	TABLE (PlatformID INT)
	DECLARE @Temp_Area	TABLE (AreaID INT)
	DECLARE @Temp_ToUser	TABLE (UserID INT)

	SET @intRtnCode = -1


	IF((@chvPlatformID IS NULL) AND (@chvAreaID IS NULL) AND (@chvToUserID IS NULL))
	BEGIN
		SET @chvErrMsg = '参数错误'
		SET @intRtnCode = -1
	END
	
	IF(@chvPlatformID <> '0')
	BEGIN
		INSERT INTO @Temp_Platform
		SELECT	CAST(Value AS INT)
		FROM [dbo].[fn_Split](@chvPlatformID,',')
	END

	IF(@chvAreaID <> '0')
	BEGIN
		INSERT INTO @Temp_Area
		SELECT	CAST(Value AS INT)
		FROM [dbo].[fn_Split](@chvAreaID,',')
	END


	IF((@chvPlatformID = '0') AND (@chvAreaID = '0'))
	BEGIN
		INSERT INTO @Temp_PlatformArea
		VALUES (0,0)
	END
	ELSE IF (@chvToUserID = '0')
	BEGIN
		INSERT INTO @Temp_PlatformArea
		VALUES (0,0)
	END
	ELSE IF ((@chvPlatformID = '0') AND (@chvAreaID <> '0'))
	BEGIN
		INSERT INTO @Temp_PlatformArea
		SELECT	DISTINCT PlatformID,AreaID
		FROM Area.Area WITH (NOLOCK)
		WHERE
			AreaID IN (SELECT AreaID FROM @Temp_Area)
	END
	ELSE IF (@chvPlatformID <> '0') AND (@chvAreaID = '0')
	BEGIN
		INSERT INTO @Temp_PlatformArea
		SELECT	DISTINCT PlatformID,AreaID
		FROM Area.Area WITH (NOLOCK)
		WHERE
			PlatformID IN (SELECT PlatformID FROM @Temp_Platform)
	END
	ELSE IF (@chvPlatformID <> '0') AND (@chvAreaID <> '0')
	BEGIN
		INSERT INTO @Temp_PlatformArea
		SELECT	DISTINCT PlatformID,AreaID
		FROM Area.Area WITH (NOLOCK)
		WHERE
			PlatformID IN (SELECT PlatformID FROM @Temp_Platform)
			AND AreaID IN (SELECT AreaID FROM @Temp_Area)
	END
	ELSE IF (@chvToUserID <> '0')
	BEGIN
		INSERT INTO @Temp_ToUser
		SELECT	CAST(Value AS INT)
		FROM [dbo].[fn_Split](@chvToUserID,',')
	END


	SET @NoticeMsgSeq = CAST(RIGHT(REPLACE(REPLACE(REPLACE(REPLACE(CONVERT(VARCHAR(23),@NowTime,121),'-',''),' ',''),':',''),'.',''),15) + '0' AS BIGINT)


	IF (@chvPlatformID IS NOT NULL) OR (@chvAreaID IS NOT NULL)
	BEGIN
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
	END
	ELSE IF (@chvToUserID IS NOT NULL)
	BEGIN
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
	END

	RETURN
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_AddMsgSystemNotify] TO [GameUser]
    AS [dbo];

