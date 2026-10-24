USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrPsWeb_UpdateUserCash]    Script Date: 2019/6/19 下午 06:05:54 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<更新用户金币>
--	2018/03/19	Jason		調整"更新金幣統計"寫法降低邏輯讀取,新增@sintSourceType判斷為比對是否有該SourceType
--	2018/03/21	Jason		調整Notify資料表寫入改為呼叫SP執行
--	2018/04/10	Jason		新增@AreaID寫入moneylog
--	2018/04/12	Jason		調整UpdateMoneySourceLog語法為呼叫SP執行及刪除判斷用戶是否已有money.bank動作改為創建帳號時一併新增
--	2018/06/07	Jason		#406 金幣統計改為LogDB計算及同步更新CenterDB
--	2018/09/27	alan		新增推廣代理流水紀錄
--	2019/06/19	Jason		新增寫入禮碼金額欄位/參數
-- =============================================
ALTER PROCEDURE [dbo].[PrPsWeb_UpdateUserCash]
(
	@intUserID			INT,					--用户ID
	@sintSourceType		SMALLINT,				--来源
	@lngUpdateAmount	BIGINT,					--更新数量
	@chvIPAddress		VARCHAR(15),			--IP
	@chvMemo			VARCHAR(128),			--说明
	@lngCurrentAmount	BIGINT			OUTPUT, --当前金币数
	@lngTaxAmount		BIGINT = 0,				--税收
	@lngFlowAmount		BIGINT = 0,				--遊戲輸贏流水,給推廣系統計算流水使用
	@intAreaID			INT = NULL,				--渠道
	@intPlatformID		INT = NULL,				--平台
	@lngCouponAmount	BIGINT = NULL
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @dtStatTime		DATETIME = GETDATE()
	DECLARE @StartStatTime	DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,GETDATE()),0)
	DECLARE @EndStatTime	DATETIME = DATEADD(DAY,DATEDIFF(DAY,0,GETDATE()),1)
	DECLARE @tnyTypeID		TINYINT = 0
	DECLARE	@lngParam		BIGINT
	DECLARE	@tnyStatus		TINYINT = 0
	
	DECLARE @UserID			INT = @intUserID
	DECLARE @SourceType		SMALLINT = @sintSourceType
	DECLARE @AreaID			INT = @intAreaID
	DECLARE @PlatformID		INT = @intPlatformID

	SET @lngCouponAmount = ISNULL(@lngCouponAmount,0)

	--更新数量为0 不执行操作
	IF @lngUpdateAmount = 0
	RETURN 1

	
	--判斷@sintSourceType是否存在 若不存在則回傳失敗
	------------------------------------------------------------------------------------------
	IF NOT EXISTS (SELECT TOP 1 1 FROM [Stat].[MoneySourceType] WITH (NOLOCK) WHERE [SourceID] = @SourceType)
	BEGIN
		RETURN -1
	END
	------------------------------------------------------------------------------------------

	
	--取用戶當下現金
	SELECT	@lngCurrentAmount = Amount FROM Money.Cash WITH (NOLOCK) WHERE UserID = @UserID

	--检查现金数量
	IF @lngCurrentAmount + @lngUpdateAmount < 0
	BEGIN
		SET @lngUpdateAmount = -1 * @lngCurrentAmount
	
		INSERT INTO Game.WarningLog
		(Memo, WriteTime)
		SELECT
		'现金不足, ' + CAST(@UserID AS VARCHAR)+',当前数量' + CAST(@lngCurrentAmount AS VARCHAR) + ',更新数量' + CAST(@lngUpdateAmount AS VARCHAR),GETDATE()
	
		IF @SourceType > 10000
		RETURN -1
	END

	--更新现金
	UPDATE Money.Cash
	SET Amount = Amount + @lngUpdateAmount,
		[CouponAmount] = [CouponAmount] + @lngCouponAmount
	WHERE UserID = @UserID
	
	--更新免費金紀錄
	IF @lngUpdateAmount <> 0 and @SourceType in (10001,10002,21001,21003,11099) --注册送/升级/登入奖励金
	BEGIN
		UPDATE Users.UserInfo SET SumFreeMoney=SumFreeMoney+@lngUpdateAmount where UserID=@UserID
	END

	--更新club推廣金紀錄
	IF @lngUpdateAmount <> 0 and @SourceType in (14001) --club轉帳
	BEGIN
		UPDATE Users.UserInfo SET SumClubMoney=SumClubMoney+@lngUpdateAmount where UserID=@UserID
	END

	--更新管理員增減金幣紀錄
	IF @lngUpdateAmount <> 0 and @SourceType in (13000,13001) --管理員增減金幣
	BEGIN
		UPDATE Users.UserInfo SET SumAdminMoney=SumAdminMoney+@lngUpdateAmount where UserID=@UserID
	END


	/*  --写日志  */
	------------------------------------------------------------------------------------------
	IF @lngUpdateAmount <> 0 OR @lngCouponAmount <> 0
	BEGIN
		EXECUTE LOGDBLINKEDSERVER.LogDB.dbo.PrPsWeb_AddMoneyLog
		   @intUserID			= @UserID
		  ,@intSourceTypeID		= @SourceType
		  ,@lngCurrentAmount	= @lngCurrentAmount
		  ,@lngUpdateAmount		= @lngUpdateAmount	  
		  ,@chvIP				= @chvIPAddress
		  ,@chvMemo				= @chvMemo
		  ,@lngTaxAmount		= @lngTaxAmount
		  ,@lngFlowAmount		= @lngFlowAmount
		  ,@intAreaID			= @AreaID
		  ,@intPlatformID		= @PlatformID
		  ,@lngCouponAmount		= @lngCouponAmount
	END
	------------------------------------------------------------------------------------------

	
	/*  --更新金币统计  */
	------------------------------------------------------------------------------------------
	--IF @SourceType >= 10000
	--BEGIN
	--	EXEC [dbo].[PrWeb_UpdateMoneySourceLog]
	--		@intUserID			= @UserID,
	--		@intServerID		= @SourceType,
	--		@lngUpdateAmount	= @lngUpdateAmount,	
	--		@lngTaxAmount		= @lngTaxAmount,	
	--		@CreateTime			= @dtStatTime
	--END
	------------------------------------------------------------------------------------------


	/*  --金币变化通知  */
	------------------------------------------------------------------------------------------
	IF @SourceType >= 10000
	BEGIN
		--属性改变通知
		SET @lngParam = @lngCurrentAmount + @lngUpdateAmount

		EXEC [NOTIFYLINKEDSERVER].[Notify].[dbo].[PrNs_Attr_Add]
			@intUserID = @UserID,
			@tnyTypeID = @tnyTypeID,
			@lngParam = @lngParam,
			@tnyStatus = @tnyStatus,
			@dtStatTime = @dtStatTime
	END
	------------------------------------------------------------------------------------------

	RETURN 1
END