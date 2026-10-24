
-- =============================================
-- Author: Eddie
-- Create date: 2019/08/02
-- Description: 首充返利活動, 增加用戶贏錢紀錄
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Event_FirstDeposit_AddWinAmount]
@intUserID			int = null,
@intUserWinAmount	bigint = null,
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

	if not exists (SELECT 1 From [Event].[FirstDepositUserRecord] with(nolock) where UserID=@intUserID and EventID=@intEventID and flag=0)
	Begin
		Set @chvErrMsg = '该用户尚未加入或已领取'
		RETURN -1
	End

	--更新用戶首充返利活動資料
	Update [Event].[FirstDepositUserRecord] Set WinAmount = WinAmount + @intUserWinAmount
	where UserID=@intUserID and EventID=@intEventID

	IF @@ROWCOUNT = 0
	Begin
		Set @chvErrMsg = '更新用户活动资料失败'
		RETURN -1
	End

	--判斷是否已達領取資格
	Declare @intUpdatedWinAmount	bigint = 0

	Select @intUpdatedWinAmount=WinAmount from [Event].[FirstDepositUserRecord] where UserID=@intUserID and EventID=@intEventID

	If @intUpdatedWinAmount >= @intWinAmount	--已達領取資格
	Begin
		--更新用戶首充返利活動資料
		Update [Event].[FirstDepositUserRecord] Set Flag=1
		where UserID=@intUserID and EventID=@intEventID

		IF @@ROWCOUNT = 0
		Begin
			Set @chvErrMsg = '更新用户活动资料失败'
			RETURN -1
		End

		--发送邮件
		Declare @chvMailContent varchar(1024)
		set @chvMailContent = '恭喜您已达到【首充返利】返利金领取资格，请前往活动页面领取返利金。'

		--发送系统邮件
		EXEC PrPs_SendSysMail
			@intUserID, 
			'【首充返利】领取资格',
			@chvMailContent,
			0,
			NULL
	End

	SET @chvErrMsg = '增加成功'	
	RETURN 1

END

GO

