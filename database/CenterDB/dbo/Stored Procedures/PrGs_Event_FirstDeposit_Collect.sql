


-- =============================================
-- Author: Eddie
-- Create date: 2019/08/02
-- Description: 領取首充返利回饋金
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_FirstDeposit_Collect]
@intUserID INT = null,
@intEventID	INT = null,
@chvIPAddress		VARCHAR(40) = '0.0.0.0',	--IP
@chvErrMsg			VARCHAR(128) OUTPUT			--返回消息
AS
BEGIN
	
	Declare	@dtStartTime		datetime
	Declare @dtEndTime			datetime
	Declare @intDepositLimit	bigint
	Declare @intWinAmount		bigint
	Declare @intReturnAmount	bigint

	Declare @intUserDeposAmount	bigint
	Declare @intUserWinAmount	bigint
	Declare	@intUserFlag		tinyint
	Declare @intUserType		tinyint

	Declare @dtNow	datetime = getdate()

	--取得首充返利活動相關資料
	SELECT @dtStartTime=[StartTime],@dtEndTime=[EndTime],@intDepositLimit=[DepositLimit],@intWinAmount=[WinAmount],@intReturnAmount=[ReturnAmount]
	FROM [Event].[FirstDeposit] with(nolock) where ID=@intEventID

	if @@ROWCOUNT <> 1
	Begin
		Set @chvErrMsg = '无相关活动资料'
		RETURN -1
	End

	--取得當前該用戶首充返利活動該相關資料
	SELECT @intUserDeposAmount=[DepositAmount],@intUserWinAmount=[WinAmount],@intUserFlag=[Flag],@intUserType=B.UserType
	FROM [Event].[FirstDepositUserRecord] A with(nolock) 
	Join Users.UserInfo B with(nolock) on A.UserID = B.UserID
	where A.UserID=@intUserID and A.EventID=@intEventID

	if @@ROWCOUNT <> 1
	Begin
		Set @chvErrMsg = '无该用户首充返利活动资料'
		RETURN -1
	End

	--檢查
	If @intUserFlag = 0
	Begin
		Set @chvErrMsg = '未达领取条件'
		RETURN -1
	End

	If @intUserFlag = 2
	Begin
		Set @chvErrMsg = '用户已领取'
		RETURN -1
	End

	If @intUserType <> 1
	Begin
		Set @chvErrMsg = '用户尚未绑定'
		RETURN -1
	End

	BEGIN TRANSACTION

	--给金币
	Declare		@intSourceType		INT = 10005 					--更新方式::首充返利
	Declare		@chvMemo			VARCHAR(128) = '首充返利'
	Declare		@intReturnValue		INT		
	Declare		@lngCurrentAmount	BIGINT
	
	EXEC @intReturnValue = dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID, -- int
		    @sintSourceType = @intSourceType,
		    @lngUpdateAmount = @intReturnAmount,
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

	--更新用戶首充返利活動資料
	Update [Event].[FirstDepositUserRecord] Set Flag=2, CollectTime=@dtNow, CollectAmount=@intReturnAmount
	where UserID=@intUserID and EventID=@intEventID

	IF @@ROWCOUNT = 0
	Begin
		Set @chvErrMsg = '更新用户活动资料失败'
		RETURN -1
	End


	--发送邮件
	Declare @chvMailContent varchar(1024)
	set @chvMailContent = '恭喜，您的【首充返利】活动返利金额: '+dbo.Fn_ShowMoney(@intReturnAmount)+' 已成功领取！'

		--发送系统邮件
	EXEC PrPs_SendSysMail
		@intUserID, 
		'【首充返利】返利金',
		@chvMailContent,
		0,
		NULL

	COMMIT TRANSACTION

	SET @chvErrMsg = '领取成功'	
	RETURN 1

END

GO

