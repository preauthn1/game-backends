USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Pay_FinishedOrderShanfu]    Script Date: 2019/7/4 下午 05:26:20 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  jason
-- Create date: 20190402
-- Description:	<閃付支付>
-- Changelog:
--	2019/05/11	Jason		增加裂變檢查
--	2019/05/30	Jason		增加充值返利
--  2019/06/04  minglung	查無用戶回傳錯誤訊息"ERRORUSER"
--	2019/07/04	Mars		增加晉級VIP獎勵 PrPs_Pay_PromotionAward
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_Pay_FinishedOrderShanfu]
(
	@chvStatus		VARCHAR(1),			-- 訂單狀態 S:訂單完成 Z:訂單取消
	@intPayType		VARCHAR(20),				-- 1:支付寶 2:微信 3:銀行卡 4:支付寶紅包 5:支付寶轉帳 6:微信紅包 7:微信轉帳 
										-- 8:大額專屬通道 9:微轉銀二維碼 10:支轉銀二維碼 11:雲閃付二維碼 12:銀行卡充值
	@intUserID		INT,				-- UserID
	@chvOrderID		VARCHAR(32),		-- 代理系统订单号
	@teamOrderID	VARCHAR(64) = NULL,	-- 项目组订单号,@tnyStatus = Z 才传此字段(回傳本地訂單號)
	@chaseOrderID	VARCHAR(64) = NULL,	-- 追分订单号,status = Z 才传此字段
	@lngAmount		BIGINT,				-- 单位"分" 必须大于零 充值金额(人民币)
	@lngPresentAmount BIGINT = null,    -- 优惠金额,单位"分", 无优惠传0
	@intTime		INT,				-- 请求时间戳(秒)
	@OrderID		INT			OUTPUT,		--本地端訂單編號
	@chvErrMsg		VARCHAR(128) OUTPUT		--返回消息
)
AS
BEGIN

	DECLARE @chvIPAddress VARCHAR(40)
	DECLARE @UserID INT
	DECLARE @PlatformID INT
	DECLARE @GWID INT=7 --閃付
	DECLARE @PayTypeID INT
	DECLARE @Status TINYINT = 2
	DECLARE @TransID VARCHAR(32) = @chvOrderID
	DECLARE @TradeCode	VARCHAR(20)
	DECLARE @UserLevel	INT
	DECLARE @lngGameCashAmount BIGINT
	DECLARE @sintSourceTypeID SMALLINT
	DECLARE @chvMemo VARCHAR(200)
	DECLARE @FinishedStatus INT = 3
	DECLARE @chvTransData VARCHAR(MAX) = ''
	DECLARE @chvMailContent varchar(1024)

	SET @lngGameCashAmount = @lngAmount
	SET @lngAmount = @lngAmount/100
	
	SELECT	@UserID = UserID,
			@PlatformID = B.PlatformID,
			@chvIPAddress = [LastLogonIPAddress]
	FROM [Users].[UserInfo] AS A WITH (NOLOCK)
		INNER JOIN [Area].[Area] AS B WITH (NOLOCK) ON A.AreaID = B.AreaID
	WHERE UserID = @intUserID

	--SELECT	@GWID = A.[GWID],
	--		@PayTypeID = B.[TypeID]
	--FROM [Pay].[PlatformPayType] AS A WITH (NOLOCK)
	--	INNER JOIN [Pay].[PayType] AS B WITH (NOLOCK) ON A.GWID = B.GWID AND A.TypeID = B.TypeID
	--WHERE [PlatformID] = @PlatformID AND [TradeCode] = @intPayType


	--SELECT	@sintSourceTypeID = [SourceTypeID],
	--		@TradeCode = [TradeCode]
	--FROM [Pay].[PayType] WITH (NOLOCK)
	--WHERE
	--	[GWID] = @GWID
	--	AND [TypeID] = @PayTypeID


	select @PayTypeID = [TypeID],
			@sintSourceTypeID = [SourceTypeID],
			@TradeCode = [TradeCode]
	 from [Pay].[PayType] where [GWID] = @GWID and [TradeCode] = @intPayType 



	/* 狀態判斷  */
	---------------------------------------------------------------------------------------------------------------
	IF (@UserID IS NULL OR @UserID <> @intUserID)
	BEGIN
		SET @chvErrMsg = 'ERRORUSER'
		RETURN -1
	END

	IF (@chvStatus NOT IN ('S','Z'))
	BEGIN
		SET @chvErrMsg = '操作失败:訂單狀態錯誤'
		RETURN -2
	END

	IF (@intPayType <> @TradeCode)
	BEGIN
		SET @chvErrMsg = '操作失败:支付類別錯誤'
		RETURN -3
	END

	IF (LEN(@chvOrderID) = 0)
	BEGIN
		SET @chvErrMsg = '操作失败:代理系統訂單號錯誤'
		RETURN -4
	END

	IF(@lngAmount <= 0)
	BEGIN
		SET @chvErrMsg = '操作失败:充值金额不正确'
		RETURN -5
	END

	IF EXISTS (SELECT TOP 1 1 FROM [Pay].[OrderList] WITH (NOLOCK) WHERE [TransID] = @chvOrderID) AND (@chvStatus = 'S')
	BEGIN
		SET @chvErrMsg = '操作失败:已经完成充值订单,不能重复提交'
		RETURN 1 --視同成功
	END

	IF (@GWID IS NULL)
	BEGIN
		SET @chvErrMsg = '操作失败:支付渠道錯誤,請連絡客服'
		RETURN -7
	END

	IF EXISTS (SELECT TOP 1 1 FROM [Pay].[OrderList] WITH (NOLOCK) WHERE [Status] = @FinishedStatus AND (ID = @teamOrderID))
		AND  (@chvStatus = 'Z')
	BEGIN
		SET @chvErrMsg = '操作失败:已經完成追分,請連絡客服'
		RETURN -8
	END
	---------------------------------------------------------------------------------------------------------------

	
	/*  實際新增訂單  */
	---------------------------------------------------------------------------------------------------------------
	DECLARE @Temp_WriteOrderID TABLE (ID INT)

	IF(@chvStatus = 'S')
	BEGIN

		SET @chvTransData = '创建订单:客户端  完成支付:闪付充值' + @chvTransData

		INSERT INTO [Pay].[OrderList]
		(	[PayTypeID],	[UserID],	[Amount],	[Status],	[IPAddress],	[TransID],	[TransData],	[CreateTime],	[GWID],	[LastUpdateTime])
		OUTPUT INSERTED.ID INTO @Temp_WriteOrderID
		VALUES
		(	@PayTypeID,		@UserID,	@lngAmount,	@Status,	@chvIPAddress,	@TransID,	@chvTransData,	GETDATE(),		@GWID,	GETDATE())

		IF(@@ROWCOUNT > 0)
		BEGIN
			SELECT	@OrderID = convert(int,ID) FROM @Temp_WriteOrderID
			SET @chvMemo = '用戶闪付充值'

			DECLARE @intReturnValue INT,@lngCurrentAmount BIGINT
			EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
				@intUserID = @intUserID, -- int
				@sintSourceType = @sintSourceTypeID,
				@lngUpdateAmount = @lngGameCashAmount,
				@chvIPAddress = @chvIPAddress,
				@chvMemo = @chvMemo,
				@lngCurrentAmount = @lngCurrentAmount OUTPUT,
				@chvErrMsg = @chvErrMsg OUTPUT




			--充值赠送. @lngPresentAmount參數暫時沒用到
			--IF @lngPresentAmount is not null and @lngPresentAmount>0
			--BEGIN
			--END

			--只有方舟用戶充值赠送 2%
			if not exists (select 1 from Area.UnionPlatform with (nolock) where PlatformID=@PlatformID)
			begin
				DECLARE @lngFreeCashAmount BIGINT=0
				SET @lngFreeCashAmount =  @lngGameCashAmount * 0.02

				EXEC dbo.PrWeb_UpdateUserCash
					@intUserID = @intUserID,
					@sintSourceType = 11099,
					@lngUpdateAmount = @lngFreeCashAmount,
					@chvIPAddress = @chvIPAddress,
					@chvMemo = '闪付充值优惠金',
					@lngCurrentAmount = @lngCurrentAmount OUTPUT,
					@chvErrMsg = @chvErrMsg OUTPUT	

					set @chvMailContent = '亲爱的玩家：
							您充值的金币:'+dbo.Fn_ShowMoney(@lngGameCashAmount)+'已到账, 充值返利送2%
			                                                                                                                                  
						（如有任何疑问请联系客服）'

			end
			else
			begin
				set @chvMailContent = '亲爱的玩家：
						您充值的金币:'+dbo.Fn_ShowMoney(@lngGameCashAmount)+'已到账
			                                                                                                                                  
					（如有任何疑问请联系客服）'
			end


			--遊戲內充值金額通知計算
			if exists(select 1 from Game.GameUserList with (nolock) where UserID=@intUserID)
			insert into Notify.Notify.PayFinishOrder(OrderID,UserID,Amount,TimeSeq) values(@OrderID, @intUserID,@lngGameCashAmount, dbo.Fn_GetTimeSeq())

			--充值到账提示
			INSERT INTO Notify.Notify.Notify(UserID,Type,Message)
			select @intUserID,1,'您充值的金币:'+dbo.Fn_ShowMoney(@lngGameCashAmount)+'已到账'

			--取用戶充值金額
			SELECT @UserLevel = [dbo].[Fn_ComputeGrade](SumCharge + SumAgentCharge + ABS(@lngGameCashAmount))
			FROM Users.UserInfo WITH (NOLOCK)
			WHERE
				UserID = @intUserID

			--更新用户累计充值信息 更新用戶等級
			UPDATE Users.UserInfo
			SET SumCharge =  SumCharge + ABS(@lngGameCashAmount),
				GradeID = @UserLevel
			WHERE UserID = @intUserID



			--发送系统邮件
			EXEC PrPs_SendSysMail
						@intUserID, 
						'充值成功',
						@chvMailContent,
						0,
						NULL


			--如果是充值返利, 取贈送比例贈送
			--DECLARE @Rate FLOAT

			--EXEC [Club02].[dbo].[PrWeb_Event_ChargeBonusRateByUser_Get]
			--	@intUserID = @intUserID,
			--	@Rate = @Rate	OUTPUT,
			--	@chvMsg= ''

			--IF(@Rate > 0)
			--BEGIN
			--	SET @lngFreeCashAmount =  @lngGameCashAmount * @Rate
			--	EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
			--		@intUserID = @intUserID,
			--		@sintSourceType = 11099,
			--		@lngUpdateAmount = @lngFreeCashAmount,
			--		@chvIPAddress = @chvIPAddress,
			--		@chvMemo = '充值返利返还金',
			--		@lngCurrentAmount = @lngCurrentAmount OUTPUT,
			--		@chvErrMsg = @chvErrMsg OUTPUT	
			--END

			----充值返利活動 無開啟的平台不予新增 
			--EXEC Club02.[dbo].[PrWeb_Event_ChargeBonusEvent]
			--	@intUserID = @intUserID,
			--	@lngChargeAmount = @lngAmount,
			--	@chvMsg = ''

			--晉級VIP獎勵
			EXEC [dbo].[PrPs_Pay_PromotionAward] 
				@intUserID, 
				@chvIPAddress,
				NULL

		END
	END	

	IF(@chvStatus = 'Z')
	BEGIN

		--Set Output OrderID
		SET @OrderID=convert(int,@teamOrderID)

		if exists (select 1 from Money.Cash where userid=@intUserID and Amount<ABS(@lngGameCashAmount))
		begin	
			SET @chvErrMsg = 'FAILAMOUNT' --馀额不足(FAILAMOUNT字串不能调整)
			RETURN -1
		end

		SET @lngGameCashAmount = 0 - @lngGameCashAmount
		SET @chvMemo = '用戶追分扣款, 支付订单号' + @teamOrderID


		EXEC dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID, 
			@sintSourceType = @sintSourceTypeID,
			@lngUpdateAmount = @lngGameCashAmount,
			@chvIPAddress = @chvIPAddress,
			@chvMemo = @chvMemo,
			@lngCurrentAmount = @lngCurrentAmount OUTPUT,
			@chvErrMsg = @chvErrMsg OUTPUT


		SET @chvTransData = '闪付訂單追分:' + @chaseOrderID + ' 金额:' + convert(varchar,@lngAmount)

		--更新订单状态
		UPDATE Pay.OrderList
		SET Status = 3
		,Amount = Amount-@lngAmount
		,TransData = TransData + ' ' + @chvTransData
		,[LastUpdateTime] = GETDATE()
		WHERE Id = @teamOrderID AND Status=2


		--取用戶充值金額
		SELECT @UserLevel = [dbo].[Fn_ComputeGrade](SumCharge + SumAgentCharge + ABS(@lngGameCashAmount))
		FROM Users.UserInfo WITH (NOLOCK)
		WHERE
			UserID = @intUserID

		--更新用户累计充值信息 更新用戶等級
		UPDATE Users.UserInfo
		SET SumCharge =  SumCharge + ABS(@lngGameCashAmount),
			GradeID = @UserLevel
		WHERE UserID = @intUserID


	END


	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''

		
	SET @chvErrMsg = '操作成功'	
	RETURN 1
END