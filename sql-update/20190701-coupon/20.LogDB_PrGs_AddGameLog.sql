USE [LogDB]
GO
/****** Object:  StoredProcedure [dbo].[PrGs_AddGameLog]    Script Date: 2019/6/19 下午 06:15:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<xxx>
-- Create date: <2016-11-8>
-- Description:	<写用户桌子记录>
--	2018/03/29	Jason	新增@intGameID @intAreaID @intPlatformID及欄位
--	2018/04/19	Jason	調整為紀錄TradeID並執行更新或新增(@lngTradeID IS NULL之後得刪除)
--	2019/05/22	Jason	新增流水欄位/寫入每日紀錄
--	2019/06/19	Jason	新增寫入禮碼金額欄位/參數
-- =============================================
ALTER PROCEDURE [dbo].[PrGs_AddGameLog]
	@intUserID				INT,			--用户ID
	@intServerID			INT,			--房间ID
	@lngLoginMoney			BIGINT,			--登录时房卡数量
	@lngAmount				BIGINT,			--修改房卡数量
	@lngTaxAmount			BIGINT,			--税收数量
	@intWinCount			INT,
	@intLoseCount			INT,
	@intDrawCount			INT,
	@intGameTime			INT,			--游戏时长
	@chvIPAddress			VARCHAR(15),	--IP地址
	@chvHDCode				VARCHAR(64),	--设备码
	@tnyHDType				TINYINT,		--设备类型
	@dtLoginTime			DATETIME,		--进房间时间
	@dtLogoutTime			DATETIME,		--退房间时间
	@intGameID				INT = NULL,		--游戏ID
	@intAreaID				INT = NULL,		--渠道ID
	@intPlatformID			INT = NULL,		--平台ID
	@lngTradeID				BIGINT = NULL,	--桌號ID
	@lngFlowAmount			BIGINT = NULL,	--流水帳
	@lngCouponAmount		BIGINT = NULL	--禮碼金額

AS
BEGIN

	SET NOCOUNT ON

	DECLARE @TradeID		BIGINT = @lngTradeID
	DECLARE @UserID		INT = @intUserID

	SET @lngCouponAmount = ISNULL(@lngCouponAmount,0)

	IF EXISTS (SELECT TOP 1 1 FROM Game.GameLog WHERE [TradeID] = @TradeID AND [UserID] = @UserID)
	BEGIN
		UPDATE Game.GameLog
		SET Amount = @lngAmount,
			TaxAmount = @lngTaxAmount,
			WinCount = @intWinCount,
			LoseCount = @intLoseCount,
			DrawCount = @intDrawCount,
			GameTime = @intGameTime,
			LogoutTime = @dtLogoutTime,
			[CouponAmount] = @lngCouponAmount
		WHERE
			[TradeID] = @TradeID
			AND [UserID] = @UserID
	END
	ELSE
	BEGIN
		INSERT INTO Game.GameLog
			( 
				UserID,	ServerID,		LoginMoney,		Amount,		TaxAmount,		WinCount,		LoseCount,		DrawCount,		GameTime,		IPAddress,
				HDCode,		HDType,			LoginTime,		LogoutTime,		GameID,		AreaID,		PlatformID,		[TradeID],		[FlowAmount],	[CouponAmount]
			)
		VALUES
			(
				@intUserID,	@intServerID,	@lngLoginMoney,	@lngAmount,	@lngTaxAmount,	@intWinCount,	@intLoseCount,	@intDrawCount,	@intGameTime,	@chvIPAddress,	
				@chvHDCode,	@tnyHDType,		@dtLoginTime,	@dtLogoutTime,	@intGameID,	@intAreaID,	@intPlatformID,	@lngTradeID,	@lngFlowAmount,	@lngCouponAmount
			)
	END


	EXEC [dbo].[PrWeb_Stat_AmountLog_Update]
		@intPlatformID	= @intPlatformID,
		@intAreaID = @intAreaID,
		@intUserID = @intUserID,
		@intType = 3,	-- 1:charge 2:convert 3:game
		@lngAmount = @lngAmount,
		@lngFlowAmount = @lngFlowAmount,	-- for game flow amount
		@lngTaxAmount = @lngTaxAmount,	-- for game tax and convert tax
		@lngCouponAmount = @lngCouponAmount	-- for couponamount used

END
