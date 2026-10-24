USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Pay_FinishedOrderByCS]    Script Date: 2019/7/4 下午 05:26:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<客服手動補單支付金額>
-- Changelog:
--	2019/05/11	Jason		增加裂變檢查
--	2019/05/30	Jason	增加充值返利
--	2019/07/04	Mars		增加晉級VIP獎勵 PrPs_Pay_PromotionAward
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_Pay_FinishedOrderByCS]
	@intAdminID			INT,					--後台操作者ID
	@intOrderID			INT,					--订单ID	
	@lngAmount			BIGINT,					--充值補償金额(帶入人民币)
	@chvMemo			VARCHAR(500),			--客服備註
	@chvIPAddress		VARCHAR(50),
	@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN

	DECLARE @tintStatus TINYINT,
	@lngPayAmount BIGINT,
	@lngGameCashAmount BIGINT,
	@intUserID INT,
	@sintSourceTypeID SMALLINT,
	@lngFreeCashAmount BIGINT,
	@intPayTypeID int

	SET @lngFreeCashAmount=0
	
	SELECT
		@intPayTypeID = A.PayTypeID
		,@sintSourceTypeID = B.SourceTypeID
		,@intUserID = UserID
		,@lngGameCashAmount = @lngAmount * B.Scale
		,@tintStatus = A.Status 
	FROM Pay.OrderList AS A LEFT JOIN Pay.PayType AS B ON A.PayTypeID = B.TypeID
	WHERE Id = @intOrderID
	
	IF @@ROWCOUNT = 0 
	BEGIN
		SET @chvErrMsg = '操作失败:订单不存在'
		RETURN -1
	END
	
	IF @lngAmount <= 0
	BEGIN
		SET @chvErrMsg = '操作失败:补单金额不正确'
		RETURN -1
	END
	
	declare @chvAdminName varchar(50)
	select @chvAdminName=LoginName from AdminSys.Admin.Users with (nolock) where UserID=@intAdminID


	--更新订单状态
	UPDATE Pay.OrderList
	SET 
	ClaimAmount = ClaimAmount + @lngAmount --累計客服補單金額
	,TransData = TransData + ', 客服(' + @chvAdminName + ')补单金額:' + CONVERT(varchar,@lngAmount)
	WHERE Id = @intOrderID

	--如果有更新
	IF @@rowcount>0
	BEGIN
		DECLARE @chvUpdCashMemo varchar(500) = '客服(' + @chvAdminName + ')补单, 单号:' + convert(varchar,@intOrderID) +  '金额:' + convert(varchar,@lngAmount)
		DECLARE @intReturnValue INT,@lngCurrentAmount BIGINT
		EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID, 
		    @sintSourceType = 11111, --客服支付补单 @sintSourceTypeID,
		    @lngUpdateAmount = @lngGameCashAmount, --金額單位"分"
		    @chvIPAddress = @chvIPAddress,
		    @chvMemo = @chvUpdCashMemo,
		    @lngCurrentAmount = @lngCurrentAmount OUTPUT,
		    @chvErrMsg = @chvErrMsg OUTPUT		   							


			--如果是久聊, 推廣期間送1%
			IF @intPayTypeID = 6 
			BEGIN
				SET @lngFreeCashAmount =  @lngGameCashAmount * 0.01
				EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
					@intUserID = @intUserID,
					@sintSourceType = 11099, --
					@lngUpdateAmount = @lngFreeCashAmount,
					@chvIPAddress = @chvIPAddress,
					@chvMemo = '久聊充值返还金',
					@lngCurrentAmount = @lngCurrentAmount OUTPUT,
					@chvErrMsg = @chvErrMsg OUTPUT	
			END


		--记录客服补单记录
		insert into Pay.CSOrderLog(CSAdminId,CSName,OrderID,UserID,Amount,Memo,CreateTime)
		values(@intAdminID, @chvAdminName, @intOrderID, @intUserID, @lngAmount, @chvMemo, getdate())



		--遊戲內充值金額通知計算
		if exists(select 1 from Game.GameUserList with (nolock) where UserID=@intUserID)
			insert into Notify.Notify.PayFinishOrder(OrderID,UserID,Amount,TimeSeq) values(@intOrderID, @intUserID,@lngGameCashAmount+@lngFreeCashAmount, dbo.Fn_GetTimeSeq())
		

		--充值到账提示
		INSERT INTO Notify.Notify.Notify(UserID,Type,Message)
		select @intUserID,1,'您充值的金币:'+dbo.Fn_ShowMoney(@lngGameCashAmount+@lngFreeCashAmount)+'已到账'

		
		--取用戶充值金額
		DECLARE @UserLevel	INT
		SELECT @UserLevel = [dbo].[Fn_ComputeGrade](SumCharge + SumAgentCharge + ABS(@lngGameCashAmount))
		FROM Users.UserInfo WITH (NOLOCK)
		WHERE
			UserID = @intUserID
		    
		--更新用户累计充值信息 更新用戶等級
		UPDATE Users.UserInfo
		SET SumCharge =  SumCharge + ABS(@lngGameCashAmount),
			GradeID = @UserLevel
		WHERE UserID = @intUserID

		
		--发送到账邮件
		declare @chvMailContent varchar(1024)
		set @chvMailContent = '亲爱的玩家：
				您充值的金币:'+dbo.Fn_ShowMoney(@lngGameCashAmount+@lngFreeCashAmount)+'已到账
			                                                                                                                                  
			（如有任何疑问请联系客服）'

		--发送系统邮件
		EXEC PrPs_SendSysMail
					@intUserID, 
					'充值成功',
					@chvMailContent,
					0,
					NULL


		--活動充值紀錄
		--declare @lngTotal bigint=@lngGameCashAmount+@lngFreeCashAmount
		--EXEC dbo.[PrGs_Event_AddContestOrderLog] @intUserID, @lngTotal

		--如果是充值返利, 取贈送比例贈送
		DECLARE @Rate FLOAT

		EXEC [Club02].[dbo].[PrWeb_Event_ChargeBonusRateByUser_Get]
			@intUserID = @intUserID,
			@Rate = @Rate	OUTPUT,
			@chvMsg= ''

		IF(@Rate > 0)
		BEGIN
			SET @lngFreeCashAmount =  @lngGameCashAmount * @Rate
			EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
				@intUserID = @intUserID,
				@sintSourceType = 11099,
				@lngUpdateAmount = @lngFreeCashAmount,
				@chvIPAddress = @chvIPAddress,
				@chvMemo = '充值返利返还金',
				@lngCurrentAmount = @lngCurrentAmount OUTPUT,
				@chvErrMsg = @chvErrMsg OUTPUT	
		END

		--充值返利活動 無開啟的平台不予新增 
		EXEC Club02.[dbo].[PrWeb_Event_ChargeBonusEvent]
			@intUserID = @intUserID,
			@lngChargeAmount = @lngAmount,
			@chvMsg = ''



		--晉級VIP獎勵
		EXEC [dbo].[PrPs_Pay_PromotionAward] 
			@intUserID, 
			@chvIPAddress,
			NULL
	END
	ELSE
	BEGIN
		SET @chvErrMsg = '操作失败:订单更新失败'
		RETURN -1	
	END	


	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''

		
	SET @chvErrMsg = '操作成功'	
	RETURN 1
END