USE [LogDB]
GO
/****** Object:  StoredProcedure [dbo].[PrPsWeb_AddMoneyLog]    Script Date: 2019/6/19 下午 06:11:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<xxx>
-- Create date: <2016-11-8>
-- Description:	<写用户房卡记录>
--	2018/04/10	Jason		新增@intAreaID參數
--	2018/04/19	Jason	調整為紀錄TradeID並執行更新或新增(@lngTradeID IS NULL之後得刪除)
--	2018/05/09	Jason	新增條件@TradeID > 0
--	2018/09/27	alan	新增推廣代理流水紀錄
--	2019/06/19	Jason		新增寫入禮碼金額欄位/參數
-- =============================================
ALTER PROCEDURE [dbo].[PrPsWeb_AddMoneyLog]
	@intUserID			INT,			--用户ID
	@intSourceTypeID	INT,			--流转类型	
	@lngCurrentAmount	BIGINT,			--当前金额
	@lngUpdateAmount	BIGINT,			--修改数量	
	@chvIP				VARCHAR(15),	--IP地址
	@chvMemo			VARCHAR(128),	--备注
	@lngTaxAmount		bigint=0,		--税收
	@lngFlowAmount		BIGINT = 0,		--遊戲輸贏流水,給推廣系統計算流水使用
	@intAreaID			INT = NULL,		--渠道
	@intPlatformID		INT = NULL,		--平台
	@lngTradeID			BIGINT = NULL,	--桌號
	@lngCouponAmount	BIGINT = NULL	--禮碼金額
AS

BEGIN

	SET NOCOUNT ON

	DECLARE @NowTime	DATETIME = GETDATE()
	DECLARE @UserID		INT = @intUserID
	DECLARE @TradeID	BIGINT = ISNULL(@lngTradeID,0)
	
	IF EXISTS (SELECT TOP 1 1 FROM [Money].[MoneyLog] with (nolock) WHERE [TradeID] = @TradeID AND @TradeID > 0 AND [UserID] = @UserID)
	BEGIN
		UPDATE [Money].[MoneyLog]
		SET [CurrentAmount] = @lngCurrentAmount,
			[UpdateAmount] = @lngUpdateAmount,
			[TaxAmount] = @lngTaxAmount,
			[FlowAmount] = @lngFlowAmount,
			[Memo] = @chvMemo,
			[CouponAmount] = @lngCouponAmount
		WHERE
			[TradeID] = @TradeID
			AND [UserID] = @UserID
	END
	ELSE
	BEGIN
		INSERT INTO [Money].[MoneyLog]( [UserID], [SourceTypeID], [CurrentAmount], [UpdateAmount], [TaxAmount], [FlowAmount], [IPAddress], [Memo], [CreateTime], [AreaID], [PlatformID], [TradeID],[CouponAmount])
		VALUES(@intUserID,	@intSourceTypeID, @lngCurrentAmount, @lngUpdateAmount, @lngTaxAmount, @lngFlowAmount, @chvIP, @chvMemo,	@NowTime, @intAreaID, @intPlatformID, @TradeID,@lngCouponAmount)
	END

	--推廣代理系統抽成, 遊戲輸贏金額流水2.5% 當成推廣代理的流水
	IF @intSourceTypeID < 10000 --属于游戏类
		and @lngFlowAmount > 0  --游戏输赢金额流水>0
		and (Select UserType from CenterDB.Users.UserInfo with (nolock) where UserID=@intUserID) <> 2 --不含機器人
	BEGIN
		EXEC Club.dbo.PrPs_AddMoneyFlows @intUserID, @lngFlowAmount
		--if 
		--(
		--	select distinct platformID from CenterDB.Area.Area with (nolock) where 
		--	AreaID = (Select AreaId from CenterDB.Users.UserInfo with (nolock) where UserID=@intUserID)
		--) 
		--in (select PlatformID from CenterDB.Area.UnionPlatform) --聯運包

		--begin
		--	EXEC Club02.dbo.PrPs_AddMoneyFlows @intUserID, @lngFlowAmount
		--end
		--else
		--begin
			--EXEC Club.dbo.PrPs_AddMoneyFlows @intUserID, @lngFlowAmount
		--end
	END
END
