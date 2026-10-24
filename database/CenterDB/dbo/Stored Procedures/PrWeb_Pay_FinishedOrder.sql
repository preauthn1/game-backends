
-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<完成支付>
-- Changelog:
--	2018/3/26 harry		add @chvTransID
--	2018/4/9  alan		修正用户累计充值金額, ABS(@lngPayAmount) -> ABS(@lngGameCashAmount), 改用"分"紀錄後台顯示才會正確
--	2018/05/10	Jason	#87 依運營需求調整等級判斷  新增充值時一併更新用戶等級
--	2018/06/29	alan	#541 orderlist新增LastUpdateTime欄位, 充值完成時更新最後更新時間
--	2019/05/11	Jason	增加裂變檢查
--	2019/05/30	Jason	增加充值返利
--	2019/06/24	Mars	增加晉級VIP獎勵 PrPs_Pay_PromotionAward
--	2019/08/02	Eddie	增加首充返利活動 PrPs_Event_FirstDeposit_Join
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_FinishedOrder]
	@intOrderID			INT,					--订单ID	
	@lngAmount			BIGINT,					--充值金额(人民币)
	@chvIPAddress		VARCHAR(15),			--IP地址
	@chvTransID			VARCHAR(32),			--第三方订单号
	@chvTransData		VARCHAR(max),			--第三方数据	
	@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN

	DECLARE @tintStatus TINYINT,
	@lngPayAmount BIGINT,
	@lngGameCashAmount BIGINT,
	@intUserID INT,
	@sintSourceTypeID SMALLINT,
	@chvMemo VARCHAR(128),
	@dtOrderCreateTime DATETIME,
	@lngFreeCashAmount BIGINT,
	@intPayTypeID int,
	@intErrorLogID int = null

	SET @lngFreeCashAmount=0
	
	SELECT
		@intPayTypeID = A.PayTypeID,
		@chvMemo = B.TypeName,
		@sintSourceTypeID = B.SourceTypeID,
		@intUserID = UserID,
		@lngPayAmount = A.Amount,
		@lngGameCashAmount = A.Amount * B.Scale
		,@tintStatus = A.Status 
		,@dtOrderCreateTime = CreateTime
	FROM Pay.OrderList AS A LEFT JOIN Pay.PayType AS B ON A.PayTypeID = B.TypeID
	WHERE Id = @intOrderID AND CreateTime > DATEADD(WW,-1,GETDATE())	--由于做了分区，只处理一周以内的订单
	
	IF @lngAmount <> @lngPayAmount
	BEGIN
		SET @chvErrMsg = '操作失败:充值金额不正确'
		RETURN -1
	END
	
	IF @lngPayAmount IS NULL
	BEGIN
		SET @chvErrMsg = '操作失败:充值渠道有问题,请联系客服人员'
		RETURN -1	
	END
	
	IF @tintStatus <>1
	BEGIN
		SET @chvErrMsg = '操作失败:已经完成充值订单,不能重复提交'
		RETURN -1	
	END

	BEGIN TRANSACTION

	--更新订单状态
	UPDATE Pay.OrderList
	SET Status = 2
		,TransID = @chvTransID
		,TransData = TransData + '  ' + @chvTransData
		,LastUpdateTime = GETDATE()		
	WHERE Id = @intOrderID AND Status=1 AND CreateTime > DATEADD(WW,-1,GETDATE())

	--如果有更新
	IF @@rowcount>0
	BEGIN

		BEGIN TRY

		DECLARE @intReturnValue INT		
		DECLARE @lngCurrentAmount BIGINT

		EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID, -- int
		    @sintSourceType = @sintSourceTypeID,
		    @lngUpdateAmount = @lngGameCashAmount,
		    @chvIPAddress = @chvIPAddress,
		    @chvMemo = @chvMemo,
		    @lngCurrentAmount = @lngCurrentAmount OUTPUT,
		    @chvErrMsg = @chvErrMsg OUTPUT	
			
			IF @intReturnValue < 1
			BEGIN
				IF @@TRANCOUNT > 0
					ROLLBACK
				SET @chvErrMsg = '操作失败:更新金币失败'
				RETURN -1
			END

			--如果是久聊, 推廣期間送1%
			IF @intPayTypeID = 6 
			BEGIN
				SET @lngFreeCashAmount =  @lngGameCashAmount * 0.01
				EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
					@intUserID = @intUserID,
					@sintSourceType = 11099,
					@lngUpdateAmount = @lngFreeCashAmount,
					@chvIPAddress = @chvIPAddress,
					@chvMemo = '久聊充值返还金',
					@lngCurrentAmount = @lngCurrentAmount OUTPUT,
					@chvErrMsg = @chvErrMsg OUTPUT	

				IF @intReturnValue < 1
				BEGIN
					IF @@TRANCOUNT > 0
						ROLLBACK
					SET @chvErrMsg = '操作失败:更新返还金失败'
					RETURN -1
				END
			END

		END TRY
		BEGIN CATCH
			IF @@TRANCOUNT > 0
				ROLLBACK

			EXEC dbo.PrPs_LogError
			RETURN -1
		END CATCH

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
		declare @lngTotal bigint=@lngGameCashAmount+@lngFreeCashAmount
		EXEC dbo.[PrGs_Event_AddContestOrderLog] @intUserID, @lngTotal


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

		--首充返利活動
		EXEC [dbo].[PrPs_Event_FirstDeposit_Join]
			@intUserID,
			@lngGameCashAmount,
			@chvErrMsg = ''

		COMMIT;	
	END
	ELSE
	BEGIN
		IF (@@TRANCOUNT > 0)		
			ROLLBACK

		SET @chvErrMsg = '操作失败:已经完成充值订单,不能重复提交'
		RETURN -1	
	END	

	

	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''

		
	SET @chvErrMsg = '操作成功'	
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_FinishedOrder] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_FinishedOrder] TO [xbuser]
    AS [dbo];

