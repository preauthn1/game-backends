





-- =============================================
-- Author:  alan
-- Create date: 2018/08/9
-- Description:	加入成为某个代理的下线
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_JoinUser]
	@intUserID			INT, 
	@chvInviteCode		VARCHAR(20),
	@chvErrMsg			Varchar(50) OUT
AS
BEGIN

	Declare @ParentID uniqueidentifier
	Declare @ParentStatus tinyint
	Declare @intPlatformID int
	Declare @intAreaID int

	--检查是否已经綁定
	IF EXISTS(Select 1 FROM dbo.AgentUsers With(nolock) WHERE USERID = @intUserID)
	BEGIN
		SET @chvErrMsg = '您的帐号已经绑定代理, 无法加入'
		RETURN -1
	END
 

	IF len(@chvInviteCode) < 5
	BEGIN
		SET @chvErrMsg = '邀请码无效'
		RETURN -1
	END


	--取得邀请码所属代理
	Select @ParentID = ID, @ParentStatus = [Status] From dbo.AgentUsers With(nolock) WHERE LOWER(InviteCode) = LOWER(@chvInviteCode)
	IF @@ROWCOUNT = 0
	BEGIN
		SET @chvErrMsg = '邀请码无效'
		RETURN -1
	END

 
	--檢查所屬代理狀態
	IF @ParentStatus <> 1
	BEGIN
		SET @chvErrMsg = '邀请码未启用'
		RETURN -1
	END


	--取得该用户的platformID areaID
	Select @intAreaID=AreaID From CenterDB.Users.UserInfo with(nolock) where UserID = @intUserID
	Select @intPlatformID=PlatformID From CenterDB.Area.Area with(nolock) where AreaID = @intAreaID

	DECLARE @intYYYYMMDD int
	Declare @tnyClass tinyint = 99 --玩家class
	Declare @tnyLevel tinyint = 0  --玩家无level值
	Declare @UID uniqueidentifier
	SET @UID = NEWID()
	SET @intYYYYMMDD = CAST ( CONVERT(varchar,GETDATE(),112) AS INT)

	--用户绑定代理
	Insert into AgentUsers(ID, UserID, Status, Class, Level, PlatformID, AreaID, ParentID)
	Values(@UID,@intUserID,0,@tnyClass,@tnyLevel,@intPlatformID,@intAreaID,@ParentID)
	
	SET @chvErrMsg = ''
	RETURN 1
END


