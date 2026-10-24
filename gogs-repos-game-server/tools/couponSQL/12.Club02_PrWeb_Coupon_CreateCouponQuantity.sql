USE [Club02]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_UserAgent_GenderQRCode]    Script Date: 2019/6/19 下午 01:02:57 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 2019/06/19
-- Description:	营运产礼码 (后台管理者使用)
--				一组礼码可提供多人使用一次时需要
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Coupon_CreateCouponQuantity]
(
	@intAdminID	INT,
	@intPlatformID	INT = 0,	-- 0:全平台
	@lngAmount	BIGINT,			-- 以分计算 前端需转换人民币与分的单位
	@intCodeCounts	INT,		-- 礼码组数
	@intMultiUser	INT = 0,	-- 单组礼码是否提供多人领取
	@intMultiCounts	INT = 1,	-- 单组礼码领取次数
	@chvMsg		VARCHAR(100) OUTPUT
)
AS
BEGIN
	SET NOCOUNT ON

	DECLARE @NowTime DATETIME = GETDATE()
	DECLARE @ExpiryDate	DATETIME = DATEADD(YEAR,1,GETDATE())
	DECLARE @Status INT = 0
	
	DECLARE @Temp_GetCode	TABLE ([CouponCode] CHAR(12))

	
	IF(@intAdminID <> 1) AND (@intPlatformID = 0)
	BEGIN
		SET @chvMsg = '操作错误 : 全平台礼码设定请联系管理者'
		RETURN -1
	END

	IF(@intPlatformID <> 0) AND NOT EXISTS (SELECT TOP 1 1 FROM CenterDB.Area.[Platform] WITH (NOLOCK) WHERE PlatformID = @intPlatformID)
	BEGIN
		SET @chvMsg = '操作错误 : 无此平台设定,请先至后台新增该平台'
		RETURN -1
	END

	IF(@intAdminID <> 1) AND NOT EXISTS (SELECT TOP 1 1 FROM [dbo].[AgentPlatformRelations] WITH (NOLOCK) WHERE [AdminID] = @intAdminID AND [PlatformID] = @intPlatformID)
	BEGIN
		SET @chvMsg = '操作错误 : 权限不足,请联系管理者增加权限'
		RETURN -1
	END

	IF(@lngAmount <= 0)
	BEGIN
		SET @chvMsg = '操作错误 : 礼码金额错误'
		RETURN -1
	END


	IF(@intCodeCounts <= 0)
	BEGIN
		SET @chvMsg = '操作错误 : 礼码组数错误'
		RETURN -1
	END

	/*  单组礼码提供单人领取一次  */
	IF(@intMultiUser <= 0) AND (ABS(@intMultiCounts) > 1)
	BEGIN
		SET @chvMsg = '操作错误 : 单组礼码提供单人领取时不可设定单人多次领取'
		RETURN -1
	END

	/*  单组礼码提供多人领取一次  */
	IF(@intMultiUser > 0) AND (ABS(@intMultiCounts) > 1)
	BEGIN
		SET @chvMsg = '操作错误 : 单组礼码提供多人领取时不可设定单人多次领取'
		RETURN -1
	END


	/*  取用预产礼码  */
	UPDATE TOP (@intCodeCounts) [dbo].[CouponCode]
	SET [Status] = 1
	OUTPUT DELETED.CouponCode INTO @Temp_GetCode
	WHERE
		[Status] = 0


	/*  写入礼码表  */	
	INSERT INTO [dbo].[CouponUserData] ([CouponCode],[PlatformID],[Status],[MultiUser],[MultiCount],[Amount],[ExpiryDate],[CreateTime])
	SELECT	[CouponCode],
			@intPlatformID,
			@Status,
			@intMultiUser,
			@intMultiCounts,
			@lngAmount,
			@ExpiryDate,
			@NowTime
	FROM @Temp_GetCode

	IF (@@ROWCOUNT = 0)
	BEGIN
		SET @chvMsg = '操作错误 : 产出礼码错误'
		RETURN -1
	END


	SELECT [CouponCode], Amount = @lngAmount FROM @Temp_GetCode

	SET @chvMsg = '操作完成'
	RETURN 1

END


