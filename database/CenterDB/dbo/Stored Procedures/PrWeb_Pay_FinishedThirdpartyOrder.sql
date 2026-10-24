
-- =============================================
-- Author:  alan
-- Create date: 20180905
-- Description:	完成統一第三方代理支付
-- 有两种支付方式 ：
-- 第一种是收银台[payType = 4]，
-- 这种订单是你们先生成订单号，然后再到久聊下单，最后支付中心回调你们的时候会携带这订单号，你们根据这种订单号做判断
-- 第二种是转帐[payType = 3]，
-- 这种你们没有生成订单，是由支付中心生成的订单，所以回调的时候只有支付中心的订单号，没有项目组订单号，这种情况你们直接给用户加金币就行了，不需要判断订单是否存在

-- Changelog:
--	2019/05/11	Jason		增加裂變檢查
--	2019/05/30	Jason		增加充值返利
--	2019/06/24	Mars		增加晉級VIP獎勵 PrPs_Pay_PromotionAward
--	2019/08/02	Eddie		增加首充返利活動 PrPs_Event_FirstDeposit_Join
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Pay_FinishedThirdpartyOrder]
	@intOrderType		int,					--订单类型3：转账,4：收银台
	@intOrderID			int,					--项目组订单号,转帐没有项目组订单号
	@intPlayerID		int,					--玩家ID
	@chvTransID			VARCHAR(32),			--第三方订单号 
	@lngAmount			BIGINT,					--充值金额(單位分)
	@chvIPAddress		VARCHAR(15),			--IP地址
	@intStatus			INT,					--交易結果 待支付  Waiting = 1, 已支付 Paied = 2, 支付失败 fail = 3
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
	@intReturnValue INT,
	@lngCurrentAmount BIGINT,
	@chvMailContent varchar(1024),
	@UserLevel	INT,
	@lngFreeCashAmount BIGINT=0


	IF @intStatus = 1
	BEGIN
		SET @chvErrMsg = '支付無效'
		RETURN -1
	END


	--转帐類型,不屬於方舟訂單, 不檢查訂單直接加金幣
	IF @intOrderType = 3
	BEGIN

		IF @intStatus = 3
		BEGIN
			SET @chvErrMsg = '支付失敗'
			RETURN -1
		END

		--檢查支付中心的订单号是否已經存在
		IF EXISTS(select 1 from pay.orderlist with (nolock) where TransID = @chvTransID AND CreateTime > DATEADD(WW,-1,GETDATE()))
		BEGIN
			SET @chvErrMsg = '操作失败:已经完成充值订单,不能重复提交'
			RETURN 1	
		END

		SET @intUserID = @intPlayerID
		SET @lngGameCashAmount = @lngAmount
		SET @dtOrderCreateTime = getdate()

		
		--直接寫一筆訂單到orderlist
		INSERT INTO Pay.OrderList
			(
				[PayTypeID],
				[UserID],
				[Amount],
				[Status],
				[IPAddress],
				[TransID],
				[TransData],
				[CreateTime],
				[LastUpdateTime],
				[GWID]
			)
		SELECT	6, --久聊支付
				@intUserID,
				@lngAmount/100, --人民幣表示
				2,				 --直接完成支付
				@chvIPAddress,
				@chvTransID,
				'完成转帐订单:' + @chvTransData,
				GETDATE(),
				GETDATE(),
				6  --統一支付
		
		SET @intOrderID=SCOPE_IDENTITY()

		--進行充直
		EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID,
			@sintSourceType = 11011,
			@lngUpdateAmount = @lngGameCashAmount,
			@chvIPAddress = @chvIPAddress,
			@chvMemo = '久聊转帐充值',
			@lngCurrentAmount = @lngCurrentAmount OUTPUT,
			@chvErrMsg = @chvErrMsg OUTPUT		   							

			IF @intReturnValue <>1
			BEGIN
				SET @chvErrMsg = @chvErrMsg
				RETURN -1	
			END

		--推廣期間送1%
	    SET @lngFreeCashAmount =  @lngGameCashAmount * 0.01
		EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID,
			@sintSourceType = 11099,
			@lngUpdateAmount = @lngFreeCashAmount,
			@chvIPAddress = @chvIPAddress,
			@chvMemo = '久聊充值返还金',
			@lngCurrentAmount = @lngCurrentAmount OUTPUT,
			@chvErrMsg = @chvErrMsg OUTPUT	


	END


	---------------------------------------------------------------------------------------------




	--收银台類型, 檢查訂單
	IF @intOrderType = 4
	BEGIN

		--支付失敗
		IF @intStatus = 3
		BEGIN
			--更新订单状态
			UPDATE Pay.OrderList
			SET Status = 3
			,TransID = @chvTransID
			,TransData = TransData + '  支付失败:' + @chvTransData
			WHERE ID = @intOrderID AND UserID = @intPlayerID AND Status=1 AND CreateTime > DATEADD(WW,-1,GETDATE())

			SET @chvErrMsg = '操作成功'
			RETURN 1

		END


		SELECT  
			@chvMemo = B.TypeName,
			@sintSourceTypeID = B.SourceTypeID,
			@intUserID = UserID,
			@lngPayAmount = A.Amount,
			@lngGameCashAmount = A.Amount * B.Scale
			,@tintStatus = A.Status 
			,@dtOrderCreateTime = CreateTime
		FROM Pay.OrderList AS A LEFT JOIN Pay.PayType AS B ON A.PayTypeID = B.TypeID
		WHERE A.ID = @intOrderID
		--AND CreateTime > DATEADD(WW,-1,GETDATE())	--由于做了分区，只处理一周以内的订单

		
		IF @tintStatus <>1
		BEGIN
			SET @chvErrMsg = '操作失败:已经完成充值订单,不能重复提交'
			RETURN -1	
		END

		--IF @lngGameCashAmount <> @lngPayAmount
		--BEGIN
		--	SET @chvErrMsg = '操作失败:充值金额不正确'
		--	RETURN -1
		--END
				
		IF @lngPayAmount IS NULL
		BEGIN
			SET @chvErrMsg = '操作失败:充值渠道有问题,请联系客服人员'
			RETURN -1	
		END
	


		--更新订单状态 (已支付)
		UPDATE Pay.OrderList
		SET Status = 2
			,TransID = @chvTransID
			,TransData = TransData + '  完成支付:' + @chvTransData
			,LastUpdateTime = GETDATE()		
		WHERE ID = @intOrderID AND Status=1 AND CreateTime > DATEADD(WW,-1,GETDATE())
		--如果有更新
		IF @@rowcount>0
		BEGIN

			--進行充直
			EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
				@intUserID = @intUserID,
				@sintSourceType = @sintSourceTypeID,
				@lngUpdateAmount = @lngGameCashAmount,
				@chvIPAddress = @chvIPAddress,
				@chvMemo = @chvMemo,
				@lngCurrentAmount = @lngCurrentAmount OUTPUT,
				@chvErrMsg = @chvErrMsg OUTPUT		   							

			IF @intReturnValue <>1
			BEGIN
				SET @chvErrMsg = @chvErrMsg
				RETURN -1	
			END

			--推廣期間送1%
			SET @lngFreeCashAmount =  @lngGameCashAmount * 0.01
			EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
				@intUserID = @intUserID,
				@sintSourceType = 11099,
				@lngUpdateAmount = @lngFreeCashAmount,
				@chvIPAddress = @chvIPAddress,
				@chvMemo = '久聊充值返还金',
				@lngCurrentAmount = @lngCurrentAmount OUTPUT,
				@chvErrMsg = @chvErrMsg OUTPUT	


		END
		ELSE
		BEGIN

			SET @chvErrMsg = '操作失败:已经完成充值订单,不能重复提交'
			RETURN -1	
		END	
	

	END
	
	--遊戲內充值金額通知計算
	if exists(select 1 from Game.GameUserList with (nolock) where UserID=@intUserID)
		insert into Notify.Notify.PayFinishOrder(OrderID,UserID,Amount,TimeSeq) values(@intOrderID, @intUserID,@lngGameCashAmount+@lngFreeCashAmount,dbo.Fn_GetTimeSeq())


	--充值到账提示
	INSERT INTO Notify.Notify.Notify(UserID,Type,Message)
	select @intUserID,1,'您充值的金币:'+dbo.Fn_ShowMoney(@lngGameCashAmount+@lngFreeCashAmount)+'已到账'


	--取用戶充值金額
	SELECT @UserLevel = [dbo].[Fn_ComputeGrade](SumCharge + SumAgentCharge + ABS(@lngGameCashAmount))
	FROM Users.UserInfo WITH (NOLOCK)
	WHERE UserID = @intUserID
		    
	--更新用户累计充值信息 更新用戶等級
	UPDATE Users.UserInfo
	SET SumCharge =  SumCharge + ABS(@lngGameCashAmount), GradeID = @UserLevel
	WHERE UserID = @intUserID
	
						
	--发送到账邮件
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


	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''
		
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

	SET @chvErrMsg = '操作成功'	
	RETURN 1
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_FinishedThirdpartyOrder] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Pay_FinishedThirdpartyOrder] TO [xbuser]
    AS [dbo];

