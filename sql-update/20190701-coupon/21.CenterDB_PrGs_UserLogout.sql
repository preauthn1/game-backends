USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_UserLogout]    Script Date: 2019/6/27 下午 06:25:07 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<用户登出房间>
--	2018/03/21	Jason		新增LOG寫入GameID,AreaID
--	2018/04/10	Jason		新增UpdateUserCash時一併寫入AreaID & PlatformID
--	2018/04/12	Jason		原更新金幣統計有2個相同SP使用不同參數字串執行改為定義變數呼叫SP執行
--	2018/04/22	alan		add transaction
--	2018/04/26	Jason		新增@lngTradeID參數給moneylog , gamelog寫入
--	2018/05/07	Jason		#76 使用者各遊戲最後一局輸贏紀錄@intLastWinLose
--	2018/06/07	Jason		#406 金幣統計改為LogDB計算及同步更新CenterDB
--	2018/08/27	jason		#711 新增寫入輸贏金額
--	2018/09/27	alan		新增推廣代理流水紀錄
--	2019/05/22	Jason		新增流水紀錄
--	2019/06/19	Jason		新增寫入禮碼金額欄位/參數
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_UserLogout]
(
	@intMsgID			INT,					--消息ID
	@intUserID			INT,					--用户ID
	@lngUpdateAmount	BIGINT,					--变化的金币数量
	@lngTaxAmount		BIGINT,					--税收
	@lngFlowAmount		BIGINT=0,				--遊戲輸贏流水,給推廣系統計算流水使用
	@intWinCount		INT,					--赢的次数
	@intLoseCount 		INT,					--输的次数
	@intDrawCount 		INT,					--平的次数
	@dtmLoginTime 		DATETIME,				--进入房间时间
	@dtmLogoutTime		DATETIME,				--离开房间时间
	@chvIPAddress		VARCHAR(15),			--IP
	@tnyHDType 			TINYINT,				--设备类型
	@chvHDCode 			VARCHAR(64),			--设备码
	@intGameTime		INT,					--游戏时间
	@sintGameID			SMALLINT,				--游戏ID
	@sintServerID		SMALLINT,				--服务器ID
	@lngLoginMoney		BIGINT,					--进房间时的金币数
	@tnyScoreUpdateMode TINYINT,				--写分方式 1 退房间:更新用戶金幣(UpdateUserCash), 並新增金幣異動紀錄(moneylog)
												--		   0 牌局結束:只新增金幣異動紀錄(moneylog)
	@lngTradeID			BIGINT,
	@intLastWinLose		INT = NULL,				--#76 上局輸贏結果 0輸 1贏
	@lngCouponAmount	BIGINT = NULL,			--禮碼金額使用
	@chvErrMsg			VARCHAR(128) = NULL OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON

	set xact_abort on

	DECLARE @NowTime			DATETIME = GETDATE()
	DECLARE @UserID				INT = @intUserID
	DECLARE @ServerID			INT = @sintServerID
	DECLARE @GameID				INT = @sintGameID
	DECLARE @PlatformID			INT
	DECLARE @AreaID				INT
	DECLARE @lngCurrentAmount	BIGINT = 0
	DECLARE @UserLogoutMemo		VARCHAR(128) = '退房间写分'


	--如果消息已处理则直接返回
	DECLARE @isMsgExists INT
	
	EXEC @isMsgExists = dbo.PrGs_CheckNetworkMsgIsExists @sintServerID,@intMsgID
	
	IF @isMsgExists = 1
	BEGIN
		RETURN 1
	END	


	--取用戶AreaID & PlatFormID
	SELECT	@AreaID = AreaID FROM [Users].[UserInfo] WITH (NOLOCK) WHERE [UserID] = @UserID
	SELECT	@PlatformID = [PlatformID] FROM [Area].[Area] WITH (NOLOCK) WHERE [AreaID] = @AreaID

	SET @AreaID		= ISNULL(@AreaID,0)
	SET @PlatformID	= ISNULL(@PlatformID,0)
	SET @lngCouponAmount= ISNULL(@lngCouponAmount,0)


	--机器人解锁
	EXEC dbo.PrGs_RobotLogout  @ServerID,@UserID

	BEGIN TRAN

	--金币解锁
	DELETE FROM Game.GameUserList 
	WHERE
		UserId =  @UserID 
		AND ServerID = @ServerID

	IF @@ROWCOUNT = 0
	BEGIN
		INSERT INTO Game.WarningLog
			(Memo, WriteTime)
		SELECT	'无锁写分,' + CAST(@UserID AS VARCHAR)
					+ ',money:' + CAST(@lngUpdateAmount AS varchar)
					+ ',serverid:' + CAST(@ServerID AS VARCHAR), 
				GETDATE()
	END


	/*  --退房间写分  */
	------------------------------------------------------------------------------------------------
	IF (@lngUpdateAmount <> 0 OR @lngCouponAmount <> 0) AND @tnyScoreUpdateMode = 1
	BEGIN
		EXEC dbo.PrPsWeb_UpdateUserCash 
			@intUserID			= @UserID,
			@sintSourceType		= @ServerID,
			@lngUpdateAmount	= @lngUpdateAmount,
			@chvIPAddress		= @chvIPAddress,
			@chvMemo			= @UserLogoutMemo,
			@lngCurrentAmount	= @lngCurrentAmount		OUTPUT,
			@lngTaxAmount		= @lngTaxAmount,
			@lngFlowAmount		= @lngFlowAmount,
			@intAreaID			= @AreaID,
			@intPlatformID		= @PlatformID,
			@lngCouponAmount    = @lngCouponAmount
	END
	------------------------------------------------------------------------------------------------


	--有金币变化或输赢平变化
	IF @lngUpdateAmount<>0 OR @intWinCount+@intLoseCount+@intDrawCount>0 OR @lngFlowAmount <> 0 OR (@lngCouponAmount <> 0)
	BEGIN	
		--更新用户游戏信息
		UPDATE Game.UserGameInfo
		SET WinCount = WinCount + @intWinCount,
			LoseCount = LoseCount + @intLoseCount,
			DrawCount = DrawCount  + @intDrawCount,
			GameTime = GameTime + @intGameTime,
			Score = Score + @lngUpdateAmount,
			[LastWinLose] = @intLastWinLose
		WHERE
			UserID = @UserID
			AND GameID = @GameID

		IF @@ROWCOUNT = 0
		BEGIN
			INSERT INTO Game.UserGameInfo
				(	UserID,		GameID,	WinCount,		LoseCount,		DrawCount,		Score,				GameTime	)
			SELECT	@UserID,	@GameID,@intWinCount,	@intLoseCount,	@intDrawCount,	@lngUpdateAmount,	@intGameTime
		END

		----更新金币统计
		--IF @tnyScoreUpdateMode = 1
		--BEGIN		
		--	EXEC [dbo].[PrWeb_UpdateMoneySourceLog]
		--		@intUserID			= @UserID,
		--		@intServerID		= @ServerID,
		--		@lngUpdateAmount	= @lngUpdateAmount,	
		--		@lngTaxAmount		= @lngTaxAmount,	
		--		@CreateTime			= @NowTime
		--END

		
		/*  --更新用户累计税收和等级  */
		------------------------------------------------------------------------------------------------
		IF NOT EXISTS (SELECT TOP 1 1 FROM Users.UserInfo WITH (NOLOCK) WHERE UserID = @UserID AND UserType = 2)
		BEGIN
			UPDATE Users.UserInfo
			SET SumTax = SumTax + @lngTaxAmount
				,GradeID = dbo.Fn_ComputeGrade(SumCharge + SumAgentCharge)
				,SumFlowAmount = SumFlowAmount + @lngFlowAmount
			WHERE
				UserID = @UserID
		END
		ELSE
		BEGIN
			UPDATE Users.UserInfo
			SET SumTax = SumTax + @lngTaxAmount
				,GradeID = dbo.Fn_ComputeGrade(SumCharge + SumAgentCharge)
			WHERE
				UserID = @UserID
		END
		------------------------------------------------------------------------------------------------

	
		IF (@lngUpdateAmount <> 0 OR @lngCouponAmount <> 0) AND @tnyScoreUpdateMode = 0
		BEGIN									
			--写金币记录					
			EXECUTE LOGDBLINKEDSERVER.LogDB.dbo.PrPsWeb_AddMoneyLog
				@intUserID			= @UserID,
				@intSourceTypeID	= @ServerID,
				@lngCurrentAmount	= @lngLoginMoney,
				@lngUpdateAmount	= @lngUpdateAmount,	  
				@chvIP				= @chvIPAddress,
				@chvMemo			= @UserLogoutMemo,
				@lngTaxAmount		= @lngTaxAmount,
				@lngFlowAmount		= @lngFlowAmount,
				@intAreaID			= @AreaID,
				@intPlatformID		= @PlatformID,
				@lngTradeID			= @lngTradeID,
				@lngCouponAmount	= @lngCouponAmount
		END

		--写游戏记录
		EXEC LOGDBLINKEDSERVER.LogDB.dbo.PrGs_AddGameLog 
				@intUserID		= @UserID,
				@intServerID	= @ServerID,
				@lngLoginMoney	= @lngLoginMoney,
				@lngAmount		= @lngUpdateAmount,
				@lngTaxAmount	= @lngTaxAmount,
				@intWinCount	= @intWinCount,
				@intLoseCount	= @intLoseCount,
				@intDrawCount	= @intDrawCount,
				@intGameTime	= @intGameTime,
				@chvIPAddress	= @chvIPAddress,
				@chvHDCode		= @chvHDCode,
				@tnyHDType		= @tnyHDType,
				@dtLoginTime	= @dtmLoginTime,
				@dtLogoutTime	= @dtmLogoutTime,
				@intGameID		= @GameID,
				@intAreaID		= @AreaID,
				@intPlatformID	= @PlatformID,
				@lngTradeID		= @lngTradeID,
				@lngFlowAmount	= @lngFlowAmount,
				@lngCouponAmount = @lngCouponAmount
	END


	COMMIT TRAN


	RETURN 1
END