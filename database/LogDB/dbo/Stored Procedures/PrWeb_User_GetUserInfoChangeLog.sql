-- =============================================
-- Author:  alan
-- Create date: 2019/03/27
-- Description:	用戶資料修改紀錄
--	2019/08/01	Eddie	修正搜尋條件沒有帶入用戶ID的Bug
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_GetUserInfoChangeLog]
@intAdminID		int,
@intUserID		int=null,
@intChangeType	int=null,
@dtStartTime	datetime=null,
@dtEndTime		datetime=null,
@chvIPAddress	varchar(15)=null,
@intPageIndex	INT	= 1,
@intPageSize	INT = 20,
@intPageCount	INT = 0	OUTPUT
AS
BEGIN

	SET NOCOUNT ON


	--渠道篩選
	DECLARE @Temp_AreaID TABLE (AreaID	INT)
	INSERT INTO @Temp_AreaID Select AreaID From CenterDB.Area.AreaAdminUser where AdminID=@intAdminID


	SELECT	@intPageCount = COUNT(1)
	FROM Users.UserInfoChangeLog I with (nolock) 
	left join CenterDB.Users.UserInfo U with (nolock) 
	on I.UserID=U.UserID
	where 
	U.AreaID in (select AreaID from @Temp_AreaID)
	AND (@dtStartTime IS NULL OR WriteTime >= @dtStartTime)
	AND (@dtEndTime IS NULL OR WriteTime <= @dtEndTime)
	AND (@chvIPAddress IS NULL OR I.IPAddress=@chvIPAddress)
	AND (@intChangeType IS NULL OR I.ChangeType=@intChangeType)
	

	SET @intPageCount = isnull(@intPageCount,0)

	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY I.LogID DESC) AS rowNum,
		I.LogID, I.UserID, I.ChangeType, I.OldData, I.NewData, I.IPAddress,I.Memo, I.WriteTime, 
		P.PlatformID,
		P.Name as PlatformName,
		U.AreaID,
		A.AreaName,
		U.LoginName

		from Users.UserInfoChangeLog I with (nolock) 
		left join CenterDB.Users.UserInfo U with (nolock) on I.UserID=U.UserID	
		left join CenterDB.Area.Area A with (nolock) on U.AreaID=A.AreaID
		left join CenterDB.Area.Platform P  with (nolock) on A.PlatformID=P.PlatformID
		where 
		U.AreaID in (select AreaID from @Temp_AreaID)
		AND (@dtStartTime IS NULL OR WriteTime >= @dtStartTime)
		AND (@dtEndTime IS NULL OR WriteTime <= @dtEndTime)
		AND (@chvIPAddress IS NULL OR I.IPAddress=@chvIPAddress)
		AND (@intChangeType IS NULL OR I.ChangeType=@intChangeType)
		AND (@intUserID IS NULL OR I.UserID=@intUserID)
	)
	SELECT * FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserInfoChangeLog] TO [GameUser]
    AS [dbo];

