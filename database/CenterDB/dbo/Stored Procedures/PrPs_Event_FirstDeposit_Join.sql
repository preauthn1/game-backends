
-- =============================================
-- Author: Eddie
-- Create date: 2019/08/02
-- Description: 加入首充返利活動
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Event_FirstDeposit_Join]
@intUserID			int = null,
@intDepositAmount	bigint = null,
@chvErrMsg			VARCHAR(128) OUTPUT			--返回消息
AS
BEGIN
	
	Declare @intPlatformID	int
	Declare @intAreaID		int
	Declare @intEventID		int	= null
	

	--取得該用戶的平台和渠道編號與帳號身分
	Select @intAreaID=A.AreaID, @intPlatformID=B.PlatformID from Users.UserInfo A with(nolock) 
	join Area.Area B with(nolock) on A.AreaID = B.AreaID
	where A.UserID = @intUserID

	--取得首充返利活動相關資料
	Select top 1 @intEventID=ID from [Event].[FirstDeposit] where platformID=@intPlatformID and AreaID=@intAreaID and [status]=1
	If @intEventID is null
	Begin
		Select top 1 @intEventID=ID from [Event].[FirstDeposit] where platformID=@intPlatformID and AreaID=0 and [status]=1
	End
	If @intEventID is null
	Begin
		Select top 1 @intEventID=ID from [Event].[FirstDeposit] where platformID=0 and AreaID=0 and [status]=1
	End

	IF @intEventID is null		--沒有相關活動資料
	Begin
		Set @chvErrMsg = '无相关活动资料'
		RETURN -1
	End

	Declare	@dtStartTime		datetime
	Declare @dtEndTime			datetime
	Declare @intDepositLimit	bigint
	Declare @intWinAmount		bigint
	Declare @intReturnAmount	bigint
	Declare @dtNow	datetime = getdate()

	--取得首充返利活動相關資料
	SELECT @dtStartTime=[StartTime],@dtEndTime=[EndTime],@intDepositLimit=[DepositLimit],@intWinAmount=[WinAmount],@intReturnAmount=[ReturnAmount]
	FROM [Event].[FirstDeposit] with(nolock) where ID=@intEventID

	if @@ROWCOUNT <> 1
	Begin
		Set @chvErrMsg = '无相关活动资料'
		RETURN -1
	End

	--檢查
	if @dtStartTime > @dtNow or @dtEndTime < @dtNow
	Begin
		Set @chvErrMsg = '不在活动时间范围内'
		RETURN -1
	End

	if @intDepositAmount < @intDepositLimit 
	Begin
		Set @chvErrMsg = '未达单笔充值金额条件'
		RETURN -1
	End

	if exists (SELECT 1 From [Event].[FirstDepositUserRecord] with(nolock) where UserID=@intUserID and EventID=@intEventID)
	Begin
		Set @chvErrMsg = '该用户已经加入'
		RETURN -1
	End

	--新增資料
	Insert into [Event].[FirstDepositUserRecord] ([UserID],[EventID],[JoinTime],[DepositAmount]) 
	Values (@intUserID, @intEventID, @dtNow, @intDepositAmount)

	IF @@ROWCOUNT = 0
	Begin
		Set @chvErrMsg = '新增用户活动资料失败'
		RETURN -1
	End


	--发送邮件
	Declare @chvMailContent varchar(1024)
	set @chvMailContent = '恭喜您已获得【首充返利】活动资格，累计赢钱额度达到'+dbo.Fn_ShowMoney(@intWinAmount)+'后即可领取返利金' + dbo.Fn_ShowMoney(@intReturnAmount)+'。'

		--发送系统邮件
	EXEC PrPs_SendSysMail
		@intUserID, 
		'【首充返利】活动资格',
		@chvMailContent,
		0,
		NULL

	SET @chvErrMsg = '加入成功'	
	RETURN 1

END

GO

