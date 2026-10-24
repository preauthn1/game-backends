-- =============================================
-- Author:  Jason
-- Create date: 2018/04/02
-- Description:	統計分析 - 用戶統計 - 註冊用戶查詢
--	2019/06/04	Jason		遮蔽帳號末5碼
--	2019/07/10	Eddie		修改為回傳完整LoginName，改由前端進行屏蔽
--	2019/08/08	Eddie		新增傳入用戶類型的篩選 (UserType)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetRegUserInfo]
(
	@intAdminID		INT,
	@intPlatformID	INT = NULL,	--平台ID
	@intAreaID		INT = NULL,
	@chvUserName	VARCHAR(32) = NULL,
	@intUserID		INT = NULL,
	@tnyUserType	TINYINT = NULL,
	@chvIPAddress	VARCHAR(15) = NULL,
	@dtStatTime		DATETIME = NULL,
	@dtEndTime		DATETIME = NULL,
	@tnyHDType		TINYINT = NULL,
	@tnyColumns		TINYINT = 2,		--2:創建時間
	@tnySort		TINYINT = 0,		--0:降序 1:升序
	@intPageIndex	INT	= 1,
	@intPageSize	INT = 20,
	@intPageCount	INT = 0		OUTPUT
)
AS
BEGIN
SET NOCOUNT ON

	DECLARE @UserName	VARCHAR(32) = @chvUserName
	DECLARE @UserID		INT = @intUserID
	DECLARE @IPAddress	VARCHAR(15) = @chvIPAddress
	DECLARE @StatTime	DATETIME = @dtStatTime
	DECLARE @EndTime	DATETIME = @dtEndTime
	DECLARE @HDType		TINYINT = @tnyHDType
	DECLARE @UserType	TINYINT = @tnyUserType


	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	DECLARE @Temp_AreaID TABLE (AreaID	INT)

	--筛选平台
	INSERT INTO @Temp_PlatformID
	SELECT	DISTINCT PlatformID --,AreaID,GameID
	FROM [dbo].[Fn_GetPlatformAccessList](@intAdminID)
	WHERE
		ISNULL(@intPlatformID,PlatformID) = PlatformID

	--筛选渠道
	IF EXISTS(Select 1 from Area.AreaAdminUser where AdminID=@intAdminID)
		INSERT INTO @Temp_AreaID Select AreaID From Area.AreaAdminUser where AdminID=@intAdminID
	ELSE
		INSERT INTO @Temp_AreaID Select AreaID From Area.Area where PlatformID in (Select PlatformID from @Temp_PlatformID)


	/*  回傳資料  */
	-------------------------------------------------------------------------------------------
	SELECT	@intPageCount = COUNT(1)
	FROM [Users].[UserInfo] AS U WITH (NOLOCK)
		INNER JOIN [Area].[Area] AS A WITH (NOLOCK) ON U.AreaID = A.AreaID
		INNER JOIN [Area].[Platform] AS P WITH (NOLOCK) ON A.PlatformID = P.PlatformID
		INNER JOIN @Temp_PlatformID AS Z ON P.PlatformID = Z.PlatformID
	WHERE
		(@UserName IS NULL OR U.[LoginName] = @UserName)
		AND (ISNULL(@intAreaID,A.AreaID) = A.AreaID)
		AND (@UserID IS NULL OR U.[UserID] = @UserID)
		AND (@IPAddress IS NULL OR U.[CreateIPAddress] = @IPAddress)
		AND (@StatTime IS NULL OR U.[CreateTime] >= @StatTime)
		AND (@EndTime IS NULL OR U.[CreateTime] <= @EndTime)
		AND (@HDType IS NULL OR U.[HDType] = @HDType)
		AND (@UserType IS NULL OR U.[UserType] = @UserType)
		AND U.[UserType] <> 2
		AND U.AreaID IN (Select AreaID From @Temp_AreaID)
	
	;WITH CTE AS
	(
		SELECT	ROW_NUMBER() OVER (ORDER BY CASE	WHEN  @tnyColumns = 0 AND @tnySort = 0 THEN (U.sumcharge+U.SumAgentCharge)
												WHEN  @tnyColumns = 0 AND @tnySort = 1 THEN (0 - (U.sumcharge+U.SumAgentCharge))
												WHEN  @tnyColumns = 1 AND @tnySort = 0 THEN U.sumconvert
												WHEN  @tnyColumns = 1 AND @tnySort = 1 THEN (0 - U.sumconvert)
												WHEN  @tnyColumns = 2 AND  @tnySort = 0 THEN DATEDIFF(SECOND,'1970-01-01', U.CreateTime)
											    WHEN  @tnyColumns = 2 AND  @tnySort = 1 THEN 0 - DATEDIFF(SECOND,'1970-01-01', U.CreateTime)
											END DESC) AS rowNum,
				[PlatformName] = P.Name,
				A.[AreaName],
				Id = U.[UserID],
				--[LoginName] = LEFT(LEN(U.[LoginName]),LEN(U.[LoginName]) - 5) + '*****',
				U.[LoginName],
				U.GradeID,
				SumCharge = U.SumCharge + U.SumAgentCharge,
				CashAmount = C.Amount,
				U.Sex,		-- 0:女 1:男
				U.[HDType],	-- 1:PC 2:安卓 3:苹果
				U.[HDCode],
				U.[UserType],	-- 1:正式账号 2:机器人账号 3:快速登录（游客账号）
				U.CreateTime,
				U.CreateIPAddress,
				U.LastLogonTime,
				U.LastLogonIPAddress
				
		FROM [Users].[UserInfo] AS U WITH (NOLOCK)
			INNER JOIN [Area].[Area] AS A WITH (NOLOCK) ON U.AreaID = A.AreaID
			INNER JOIN [Area].[Platform] AS P WITH (NOLOCK) ON A.PlatformID = P.PlatformID
			INNER JOIN @Temp_PlatformID AS Z ON P.PlatformID = Z.PlatformID
			LEFT JOIN [Money].[Cash] AS C WITH (NOLOCK) ON U.UserID = C.UserID
		WHERE
			(@UserName IS NULL OR U.[LoginName] = @UserName)
			AND (ISNULL(@intAreaID,A.AreaID) = A.AreaID)
			AND (@UserID IS NULL OR U.[UserID] = @UserID)
			AND (@IPAddress IS NULL OR U.[CreateIPAddress] = @IPAddress)
			AND (@StatTime IS NULL OR U.[CreateTime] >= @StatTime)
			AND (@EndTime IS NULL OR U.[CreateTime] <= @EndTime)
			AND (@HDType IS NULL OR U.[HDType] = @HDType)
			AND (@UserType IS NULL OR U.[UserType] = @UserType)
			AND U.[UserType] <> 2
			AND U.AreaID IN (Select AreaID From @Temp_AreaID)
	)
	SELECT * into #tmp FROM CTE WHERE rowNum BETWEEN @intPageSize * (@intPageIndex -1) + 1 AND @intPageSize * @intPageIndex
	-------------------------------------------------------------------------------------------
	
	select *,CASE WHEN [dbo].[Fn_GetUserNickName](CreateIPAddress) = '来自火星' THEN '未知' ELSE [dbo].[Fn_GetUserNickName](CreateIPAddress) END as Location from #tmp
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetRegUserInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetRegUserInfo] TO [xbuser]
    AS [dbo];

