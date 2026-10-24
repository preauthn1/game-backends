-- =============================================
-- Author:  Jason
-- Create date: 2018/05/18
-- Description:	#271 用戶統計-註冊用戶查詢增加現金資訊與統計註冊人數,充值人數 查詢統計註冊人數,充值人數
-- Changelog:
--	2018/5/19	alan	調整計算方式
--	2018/05/21  Emily	#292 用戶統計-> 註冊用戶查詢, 上方注册人数與充值人数納入渠道與硬件類型篩選
--	2018/06/27	jason	聯運需求, 開放後台給第三方查詢特定平台與渠道營運資訊 新增@intAdminID
--	2018/07/11	alan	充值人數也要包含代理充值
--	2019/08/08	Eddie		新增傳入用戶類型的篩選 (UserType)
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_GetUserRegChargeInfo]
(
	@dtmRegBeginTime	DATETIME=null,--注册开始时间
	@dtmRegEndTime		DATETIME=null,--注册结束时间
	@intRegCount		INT = 0 OUTPUT,		--註冊人數
	@intChargeCount		INT = 0 OUTPUT,		--充值人數
	@intAreaID          INT = null,  --渠道
	@tnyUserType        Tinyint = null,   --用戶類型 1:正式账号 2:机器人账号 3:快速登录（游客账号）
	@inthdType          Tinyint = null,   --硬件類型
	@intAdminID			INT,
	@intPlatformID		INT = NULL
)
AS
BEGIN


	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	DECLARE @Temp_AreaID TABLE (AreaID	INT)

	--筛选平台
	INSERT INTO @Temp_PlatformID
	SELECT	DISTINCT PlatformID
	FROM [dbo].[Fn_GetPlatformAccessList](@intAdminID)
	WHERE
		ISNULL(@intPlatformID,PlatformID) = PlatformID

	--筛选渠道
	IF EXISTS(Select 1 from Area.AreaAdminUser where AdminID=@intAdminID)
		INSERT INTO @Temp_AreaID Select AreaID From Area.AreaAdminUser where AdminID=@intAdminID
	ELSE
		INSERT INTO @Temp_AreaID Select AreaID From Area.Area where PlatformID in (Select PlatformID from @Temp_PlatformID)


	SELECT @intRegCount=count(userid) FROM users.userinfo as a WITH (NOLOCK)
		inner join [Area].[Area] as b with (nolock) on a.AreaID = b.AreaID
		inner join @Temp_PlatformID as c on b.PlatformID = c.PlatformID
	WHERE 
		ISNULL(@dtmRegBeginTime,a.createtime) <= a.createtime AND ISNULL(@dtmRegEndTime,a.createtime) >= a.createtime
		AND ISNULL(@intAreaID, a.AreaID) = a.AreaID
		AND ISNULL(@inthdType, a.HDType) = a.HDType
		AND ISNULL(@tnyUserType, a.UserType) = a.USerType
		and a.UserType <> 2
		AND a.AreaID IN (Select AreaID From @Temp_AreaID)
	
	SELECT @intChargeCount=count(userid) FROM users.userinfo as a WITH (NOLOCK) 
		inner join [Area].[Area] as b with (nolock) on a.AreaID = b.AreaID
		inner join @Temp_PlatformID as c on b.PlatformID = c.PlatformID
	WHERE 
		(a.Sumcharge + a.SumAgentCharge) > 0 
		and ISNULL(@dtmRegBeginTime,a.createtime) <= a.createtime AND ISNULL(@dtmRegEndTime,a.createtime) >= a.createtime
		AND ISNULL(@intAreaID, a.AreaID) = a.AreaID
		AND ISNULL(@inthdType, a.HDType) = a.HDType
		AND ISNULL(@tnyUserType, a.UserType) = a.USerType
		AND a.AreaID IN (Select AreaID From @Temp_AreaID)
	
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserRegChargeInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserRegChargeInfo] TO [xbuser]
    AS [dbo];

