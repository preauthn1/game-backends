-- =============================================
-- Author:  alan
-- Create date: 2019/3/27
-- Description:	檢查後台操作者能不能查詢該用戶
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_AdminCheck]
@intUserID INT=null,
@chvLoginName varchar(32)=null,
@intAdminID	INT
AS
BEGIN
	SET NOCOUNT ON

	if @intUserID is null AND @chvLoginName is null
		return 0

	declare @intUserPlatformID int=0

	if @intUserID is not null
	begin
		--取得玩家的平台
		select @intUserPlatformID = PlatformID from Area.Area with (nolock) where areaid in (select AreaId from Users.UserInfo with (nolock) where userid=@intUserID)
		if @@ROWCOUNT=0
		begin
			return 0
		end
	end
	else
	begin
		--取得玩家的平台
		select @intUserPlatformID = PlatformID from Area.Area with (nolock) where areaid in (select AreaId from Users.UserInfo with (nolock) where LoginName=@chvLoginName)
		if @@ROWCOUNT=0
		begin
			return 0
		end
	end

	--取得管理者是否有該平台權限
	select PlatformID from [Area].[PlatformAdminUser] with (nolock) where AdminID=@intAdminID and PlatformID=@intUserPlatformID
	if @@ROWCOUNT=0
	begin
		return 0
	end

	--有權限
	return 1


END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_AdminCheck] TO [GameUser]
    AS [dbo];

