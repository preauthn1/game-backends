-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<Description,,>
--	2018/03/22	Jason		調整語法改為LINKEDSERVER呼叫回傳資料表
--  2018/04/19	alan		增加顯示欄位 ,c.UserID AS 'User.ID' ,因為前端沒有顯示
--	2018/05/09	Jason		調整INNER改LEFT對應
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_GetGameLog] 	
	@intServerID		INT = NULL,
	@intGameID			INT = NULL, 
	@intUserID			INT = NULL,
	@chvIpAddress		VARCHAR(50) = NULL,
	@intHDtype			INT = NULL,
	@chvHDcode			VARCHAR(50) = NULL,
	@logInBeginTime		DATETIME = NULL, 
	@logInEndTime		DATETIME = NULL,
	@logOutBeginTime	DATETIME = NULL,
	@logOutEndTime		DATETIME = NULL,
	@intAreaID			INT = NULL,
	@intPageIndex		INT = 1,
	@intPageSize		INT = 20,
	@intPageCount		INT	= 0			OUTPUT		
AS
BEGIN
	--select  @intPageCount=COUNT(a.LogID) from 
	--[Log].[GameLog] a
	--inner join ServerInfo.GameServerList b ON a.ServerID=b.ServerID
	--INNER JOIN Users.UserInfo c  on a.UserID=c.UserID
	--where  (@intServerID is null or a.ServerID=@intServerID)
	--		and (@intGameID is null or b.GameID=@intGameID)
	--		and (@intUserID is null or a.UserID=@intUserID)
	--		and (@chvIpAddress is null or a.IPAddress = @chvIpAddress)
	--		and (@intHDtype is null or a.HDType=@intHDtype)
	--		and (@chvHDcode is null or a.HDCode=@chvHDcode)
	--		and (@logInBeginTime is null or a.LoginTime >= @logInBeginTime)
	--		and (@logInEndTime is null or a.LoginTime <= @logInEndTime)
	--		and  a.LogoutTime >= @logOutBeginTime
	--		and (@logOutEndTime is null or a.LogoutTime <= @logOutEndTime)
	--		AND (@intAreaID IS NULL OR c.AreaID = @intAreaID)

	--;
	--with tab as (
	--	select ROW_NUMBER() over(order by a.LogoutTime desc) as rowNum,a.*
	--		,b.GameID  as 'GameServer.GameID'
	--		,b.ServerName  as 'GameServer.ServerName'
	--		,b.ShowName  as 'GameServer.ShowName'
	--		,b.ClientAddr  as 'GameServer.ClientAddr'
	--		,b.NotifyAddr  as 'GameServer.NotifyAddr'
	--		,b.Money  as 'GameServer.Money'
	--		,b.LastUpdateTime  as 'GameServer.LastUpdateTime'
	--		,c.FaceID as 'User.FaceID'
	--		,c.LoginName as 'User.LoginName'
	--		,c.NickName as 'User.NickName'
	--		,c.Password as 'User.Password'
	--		,c.Sex as 'User.Sex'
	--		,c.HDType as 'User.HDType'
	--		,c.HDCode as 'User.HDCode'
	--		,c.UserType as 'User.UserType'
	--		,c.CreateTime as 'User.CreateTime'
	--		,c.CreateIPAddress as 'User.CreateIPAddress'
	--		,c.LastLogonTime as 'User.LastLogonTime'
	--		,c.LastLogonIPAddress as 'User.LastLogonIPAddress'
	--		,e.Name as 'User.Area.Platform.Name'
	--	from [Log].[GameLog] a
	--	inner join ServerInfo.GameServerList b ON a.ServerID=b.ServerID
	--	INNER JOIN Users.UserInfo c  on a.UserID=c.UserID
	--	INNER JOIN Area.Area d on c.AreaID=d.AreaID
	--	INNER JOIN Area.Platform e on d.PlatformID=e.PlatformID
	--	where (@intServerID is null or a.ServerID=@intServerID)
	--		and (@intGameID is null or b.GameID=@intGameID)
	--		and (@intUserID is null or a.UserID=@intUserID)
	--		and (@chvIpAddress is null or a.IPAddress = @chvIpAddress)
	--		and (@intHDtype is null or a.HDType=@intHDtype)
	--		and (@chvHDcode is null or a.HDCode=@chvHDcode)
	--		and (@logInBeginTime is null or a.LoginTime >= @logInBeginTime)
	--		and (@logInEndTime is null or a.LoginTime <= @logInEndTime)
	--		and  a.LogoutTime >= @logOutBeginTime
	--		and (@logOutEndTime is null or a.LogoutTime <= @logOutEndTime)
	--		AND (@intAreaID IS NULL OR c.AreaID = @intAreaID)
	--)
	--select * from tab where rowNum between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex
	
	CREATE TABLE #Temp_UserGameLog
		(
			rowNum INT,
			[LogID] INT,
			[UserID] INT,
			[ServerID] INT,
			[LoginMoney] BIGINT,
			[Amount] BIGINT,
			[TaxAmount] BIGINT,
			[WinCount] INT,
			[LoseCount] INT,
			[DrawCount] INT,
			[GameTime] INT,
			[IPAddress] VARCHAR(15),
			[HDCode] VARCHAR(64),
			[HDType] TINYINT,
			[LoginTime] DATETIME,
			[LogoutTime] DATETIME,
			[GameID] INT,
			[AreaID] INT
		)

	INSERT INTO #Temp_UserGameLog
	EXEC [LOGDBLINKEDSERVER].[LogDB].[dbo].[PrWeb_Game_GetGameLog]
		@intServerID = @intServerID,
		@intUserID = @intUserID,
		@chvIpAddress = @chvIpAddress,
		@tnyHDtype = @intHDtype,
		@chvHDcode = @chvHDcode,
		@dtLoginStartTime = @LoginBeginTime,
		@dtLoginEndTime = @LoginEndTime,
		@dtLogoutStartTime = @LogoutBeginTime,
		@dtLogoutEndTime = @LogoutEndTime,
		@intGameID = @intGameID,
		@intAreaID = @intAreaID,
		@intPageIndex = @intPageIndex,
		@intPageSize = @intPageSize,
		@intPageCount = @intPageCount OUTPUT
	
	
	SELECT	rowNum,
			a.*
			,b.GameID  AS 'GameServer.GameID'
			,b.ServerName  AS 'GameServer.ServerName'
			,b.ShowName  AS 'GameServer.ShowName'
			,b.ClientAddr  AS 'GameServer.ClientAddr'
			,b.NotifyAddr  AS 'GameServer.NotifyAddr'
			,b.Money  AS 'GameServer.Money'
			,b.LastUpdateTime  AS 'GameServer.LastUpdateTime'
			,c.UserID AS 'User.ID'
			,c.FaceID AS 'User.FaceID'
			,c.LoginName AS 'User.LoginName'
			,c.NickName AS 'User.NickName'
			,c.Password AS 'User.Password'
			,c.Sex AS 'User.Sex'
			,c.HDType AS 'User.HDType'
			,c.HDCode AS 'User.HDCode'
			,c.UserType AS 'User.UserType'
			,c.CreateTime AS 'User.CreateTime'
			,c.CreateIPAddress AS 'User.CreateIPAddress'
			,c.LastLogonTime AS 'User.LastLogonTime'
			,c.LastLogonIPAddress as 'User.LastLogonIPAddress'
			,e.Name AS 'User.Area.Platform.Name'
	FROM #Temp_UserGameLog AS A
		LEFT JOIN ServerInfo.GameServerList AS b WITH (NOLOCK) ON a.ServerID = b.ServerID
		LEFT JOIN Users.UserInfo AS c WITH (NOLOCK) ON a.UserID = c.UserID
		LEFT JOIN Area.Area AS d WITH (NOLOCK) ON c.AreaID = d.AreaID
		LEFT JOIN Area.Platform AS e WITH (NOLOCK) ON d.PlatformID = e.PlatformID

	SET @intPageCount = ISNULL(@intPageCount,0)
	
	DROP TABLE #Temp_UserGameLog
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameLog] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_GetGameLog] TO [xbuser]
    AS [dbo];

