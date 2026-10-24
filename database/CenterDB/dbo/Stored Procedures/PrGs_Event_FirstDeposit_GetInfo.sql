
-- =============================================
-- Author: Eddie
-- Create date: 2019/08/02
-- Description: 取得首充返利活動與該用戶相關資料
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_FirstDeposit_GetInfo]
@intUserID INT = null
AS
BEGIN
	
	Declare @intPlatformID	int
	Declare @intAreaID		int
	Declare @intUserType	tinyint
	Declare @intEventID		int	= null
	Declare @intCurrentUserCount	int = 0

	--取得該用戶的平台和渠道編號與帳號身分
	Select @intAreaID=A.AreaID, @intPlatformID=B.PlatformID, @intUserType=A.UserType from Users.UserInfo A with(nolock) 
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
	   
	--取地目前參加人數(實際人數加上10123)
	Select @intCurrentUserCount = Count(ID)+10123 FROM [Event].[FirstDepositUserRecord] with(nolock) where EventID=@intEventID

	SELECT [ID],[PlatformID],[AreaID],[StartTime],[EndTime],[DepositLimit],[WinAmount],[ReturnAmount],[Status],@intCurrentUserCount as CurrentUserCount
	FROM [Event].[FirstDeposit] where ID=@intEventID

	--取得首充返利活動該用戶相關資料
	SELECT [UserID],[EventID],[JoinTime],[DepositAmount],[WinAmount],[Flag],[CollectTime],[CollectAmount], @intUserType as UserType
	FROM [Event].[FirstDepositUserRecord] where UserID=@intUserID and EventID=@intEventID

	

END

GO

