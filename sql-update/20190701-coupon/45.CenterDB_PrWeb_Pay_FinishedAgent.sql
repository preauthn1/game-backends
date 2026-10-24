USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Pay_FinishedAgent]    Script Date: 2019/7/9 下午 01:45:45 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author: alan
-- Create date: 2018/7/1
-- Description:	完成代理充值
-- Changelog:
--	2018/08/08	alan		#656 新增代理推廣系統抽成
--	2019/05/11	Jason		增加裂變檢查
--  2019/06/17	minglung	增加充值返利
--	2019/07/09	Mars	增加VIP晉級獎勵
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_Pay_FinishedAgent]
	@intUserID				INT,					--用户ID
	@sintSourceType			SMALLINT,				--金币变化类型
	@lngUpdateAmount		BIGINT,					--更新金币数	
	@chvIPAddress			VARCHAR(15),			--IP地址
	@chvMemo				VARCHAR(128),			--备注
	@lngCurrentAmount		BIGINT OUTPUT,			--修改前的金币数
	@chvErrMsg			VARCHAR(128) OUTPUT			--错误消息
AS
BEGIN
  		
	SET NOCOUNT ON

	DECLARE @intReturnValue INT
	EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
		@intUserID = @intUserID,
		@sintSourceType = @sintSourceType,
		@lngUpdateAmount = @lngUpdateAmount,
		@chvIPAddress = @chvIPAddress,
		@chvMemo = @chvMemo,
		@lngCurrentAmount = @lngCurrentAmount OUTPUT,
		@chvErrMsg = @chvErrMsg OUTPUT	

	IF @intReturnValue = 1 AND @lngUpdateAmount > 0
	BEGIN

		--遊戲內充值金額通知計算
		if exists(select 1 from Game.GameUserList with (nolock) where UserID=@intUserID)	
			insert into Notify.Notify.PayFinishOrder(OrderID,UserID,Amount,TimeSeq) values(0, @intUserID,@lngUpdateAmount,dbo.Fn_GetTimeSeq())
				
		--取用戶充值金額
		DECLARE @UserLevel	INT
		SELECT @UserLevel = [dbo].[Fn_ComputeGrade](SumCharge + SumAgentCharge + ABS(@lngUpdateAmount))
		FROM Users.UserInfo WITH (NOLOCK)
		WHERE
			UserID = @intUserID
		    
		--更新用户累计代理充值信息 更新用戶等級
		UPDATE Users.UserInfo
		SET SumAgentCharge =  SumAgentCharge + ABS(@lngUpdateAmount),
			GradeID = @UserLevel
		WHERE UserID = @intUserID			

		
		--发送到账邮件
		declare @lngFreeCashAmount	BIGINT=0
		declare @chvMailContent		varchar(1024)
		declare @chvNotifyContent	varchar(1024)
		declare @intRate			int
		
		--代理充值贈送比例
		SELECT @intRate = [Value] FROM CenterDB.ServerInfo.Config with (nolock) WHERE [Name] = 'AgentRate'
	
		SET @lngFreeCashAmount =  @lngUpdateAmount * (@intRate * 0.01)		
		 
		if @lngFreeCashAmount > 0 
		begin 					

			EXEC dbo.PrWeb_UpdateUserCash
				@intUserID = @intUserID,
				@sintSourceType = 11099,
				@lngUpdateAmount = @lngFreeCashAmount,
				@chvIPAddress = @chvIPAddress,
				@chvMemo = '代理充值优惠金',
				@lngCurrentAmount = @lngCurrentAmount OUTPUT,
				@chvErrMsg = @chvErrMsg OUTPUT	

			set @chvMailContent = '亲爱的玩家：
					您充值的金币:'+dbo.Fn_ShowMoney(@lngUpdateAmount + @lngFreeCashAmount)+'已到账, 充值返利送' + CAST(@intRate as varchar) + '%
			                                                                                                                                  
				（如有任何疑问请联系客服）'
				
			set @chvNotifyContent = '您充值的金币:'+dbo.Fn_ShowMoney(@lngUpdateAmount + @lngFreeCashAmount)+'已到账, 充值返利送' + CAST(@intRate as varchar) + '%'
		end
		else
		begin
			set @chvMailContent = '亲爱的玩家：
					您充值的金币:'+dbo.Fn_ShowMoney(@lngUpdateAmount)+'已到账
			                                                                                                                                  
				（如有任何疑问请联系客服）'

			set @chvNotifyContent = '您充值的金币:'+dbo.Fn_ShowMoney(@lngUpdateAmount)+'已到账'
		end
		

		--充值到账提示
		INSERT INTO Notify.Notify.Notify(UserID,Type,Message)
		select @intUserID,1,@chvNotifyContent

		--发送系统邮件
		EXEC PrPs_SendSysMail
					@intUserID, 
					'充值成功',
					@chvMailContent,
					0,
					NULL


		--活動充值紀錄
		--EXEC dbo.[PrGs_Event_AddContestOrderLog] @intUserID,@lngUpdateAmount

		--晉級VIP獎勵
		EXEC [dbo].[PrPs_Pay_PromotionAward] 
			@intUserID, 
			@chvIPAddress,
			NULL	
	END


	EXEC Club02.[dbo].[PrWeb_Split_BindingCheck]
		@intUserID = @intUserID,				--用户 ID
		@chvMsg	= ''
		

	RETURN @intReturnValue



END