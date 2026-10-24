-- =============================================
-- Author:  Jason
-- Create date: 2018/04/20
-- Description:	機器人設定(新刪修)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_RobotSettingModify]
(
	@intServerID	INT,		--伺服器編號
	@intGameID		INT,		--遊戲編號
	@chvBeginTime	CHAR(5),	--起始時間
	@chvEndTime		CHAR(5),	--結束時間
	@intMinCount	INT,		--最低數量
	@intMaxCount	INT,		--最高數量
	@intMinLevel	INT,		--最低智商
	@intMaxLevel	INT,		--最高智商
	@tnyStatus		TINYINT,	--狀態
	@chvErrMsg		VARCHAR(64)	OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @ServerID		INT = @intServerID
	DECLARE @GameID			INT = @intGameID
	DECLARE @OldStatus		TINYINT
	DECLARE @StartMinute	INT
	DECLARE @EndMinute		INT

	SET @StartMinute	= CAST(LEFT(@chvBeginTime,2) AS INT) * 60 + CAST(RIGHT(@chvBeginTime,2) AS INT)
	SET @EndMinute		= CAST(LEFT(@chvEndTime,2) AS INT) * 60 + CAST(RIGHT(@chvEndTime,2) AS INT)
	SET @OldStatus		= (SELECT [Status] FROM [Robot].[ServerConfig] WITH (NOLOCK) WHERE [ServerID] = @ServerID)
	SET @chvErrMsg		= ''

	IF NOT EXISTS (SELECT TOP 1 1 FROM [ServerInfo].[GameServerList] WITH (NOLOCK) WHERE [ServerID] = @ServerID AND [GameID] = @GameID)
	BEGIN
		SET @chvErrMsg	= '操作失敗：查無指定遊戲伺服器'
		RETURN -90001
	END

	--IF EXISTS (	SELECT TOP 1 1
	--			FROM [Robot].[ServerConfig] WITH (NOLOCK)
	--			WHERE
	--				[ServerID] = @ServerID
	--				AND [Status] = 1 AND @tnyStatus = 1
	--				AND	(
	--						(@StartMinute >= [StartMinute] AND @EndMinute <= [EndMinute])
	--						OR
	--						(@EndMinute >= [StartMinute] AND @EndMinute <= [EndMinute])
	--					)
	--			)
	--BEGIN
	--	SET @chvErrMsg	= '操作失敗：時間設定錯誤'
	--	RETURN -90002
	--END

	
	/*  寫入紀錄  */
	----------------------------------------------------------------------
	IF (@tnyStatus = 0)
	BEGIN
		UPDATE [Robot].[ServerConfig]
		SET [Status] = @tnyStatus
		WHERE
			[ServerID] = @ServerID
	END
	ELSE IF EXISTS (SELECT TOP 1 1 FROM [Robot].[ServerConfig] WITH (NOLOCK) WHERE [ServerID] = @ServerID)
	BEGIN
		UPDATE [Robot].[ServerConfig]
		SET [BeginTime] = @chvBeginTime,
			[EndTime] = @chvEndTime,
			[MinCount] = @intMinCount,
			[MaxCount] = @intMaxCount,
			[MinLevel] = @intMinLevel,
			[MaxLevel] = @intMaxLevel,
			[StartMinute] = @StartMinute,
			[EndMinute] = @EndMinute,
			[Status] = @tnyStatus
		WHERE
			[ServerID] = @ServerID
	END
	ELSE
	BEGIN
		INSERT INTO [Robot].[ServerConfig]
			(
				[ServerID],	[BeginTime],	[EndTime],		[MinCount],		[MaxCount],		[MinLevel],		[MaxLevel],		[StartMinute],	[EndMinute],	[Status]
			)
		VALUES
			(
				@ServerID,	@chvBeginTime,	@chvEndTime,	@intMinCount,	@intMaxCount,	@intMinLevel,	@intMaxLevel,	@StartMinute,	@EndMinute,		@tnyStatus
			)
	END

	IF @@ERROR = 0
	BEGIN
		IF (@OldStatus IS NULL OR (@OldStatus = 0 AND @tnyStatus = 1))
		BEGIN
			SET @chvErrMsg		= '操作成功：新增配置信息成功'
		END
		ELSE IF (@OldStatus = 1 AND @tnyStatus = 0)
		BEGIN
			SET @chvErrMsg		= '操作成功：刪除配置信息成功'
		END
		ELSE
		BEGIN
			SET @chvErrMsg		= '操作成功：修改配置信息成功'
		END
	END
	ELSE
	BEGIN
		SET @chvErrMsg	= '操作失敗：配置信息失敗'
	END
	----------------------------------------------------------------------

	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_RobotSettingModify] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_RobotSettingModify] TO [xbuser]
    AS [dbo];

