USE [CenterDB]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_Game_PerLog_SearchPerGameDetail]    Script Date: 2019/7/1 下午 07:03:52 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Jason
-- Create date: 2018/04/17
-- Description:	<查询每局游戏记录> 機制改寫為從CenterDB呼叫SP取分頁資料後再回傳
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_Game_PerLog_SearchPerGameDetail]
	@intUserID		INT = NULL,			--用户ID
	@intGameID		INT = NULL,			--游戏ID
	@intServerID	INT = NULL,			--服务器ID
	@longDealNo     BIGINT = NULL,		--牌局号 --牌局标识
	@dtBeginTime	DATETIME = NULL,	--开始时间范围
	@dtEndTime		DATETIME = NULL,	--结束时间范围
	@intPageIndex	INT = 1,
	@intPageSize    INT = 20,
	@intPageCount   INT			OUTPUT
AS
BEGIN

	SET NOCOUNT ON
	
	DECLARE @chvDeskCode	VARCHAR(15) = NULL	--房间口令   之後要取消

	DECLARE @Temp_PerGameLogDetail	TABLE
		(
			[Id]			BIGINT,
			[LogID]			BIGINT,
			[ServerID]		INT,
			[UserID]		INT,
			[CurrentAmount]	BIGINT,
			[UpdateAmount]	BIGINT,
			[GameTime]		DATETIME,
			--[GameID]		INT,
			[CurrentCouponAmount]	BIGINT,
			[CouponAmount]	BIGINT,
			[rowid]			INT,
			[ServerName]	VARCHAR(64),
			[GameName]		VARCHAR(64),
			[LoginName]		VARCHAR(32),
			[NickName]		VARCHAR(32),
			[GradeID]		INT,
			[PlarformName]	VARCHAR(32)
		)


	/*  暫存資料  */
	-----------------------------------------------------------------------------------------------------
	INSERT INTO @Temp_PerGameLogDetail
	EXEC [PerGameLog].[dbo].[PrWeb_PerLog_SearchPerGameDetail]
		@intUserID		= @intUserID,		--用户ID
		@intGameID		= @intGameID,		--游戏ID
		@intServerID	= @intServerID,		--服务器ID
		@longDealNo     = @longDealNo,		--牌局号 --牌局标识
		@chvDeskCode	= @chvDeskCode,		--房间口令
		@dtBeginTime	= @dtBeginTime,		--开始时间范围
		@dtEndTime		= @dtEndTime,		--结束时间范围
		@intPageIndex	= @intPageIndex,
		@intPageSize    = @intPageSize,
		@intPageCount   = @intPageCount			OUTPUT
	-----------------------------------------------------------------------------------------------------


	/*  回傳資料  */
	-----------------------------------------------------------------------------------------------------
	SELECT	A.[Id],
			A.[LogID],
			A.[ServerID],
			A.[UserID],
			A.[CurrentAmount],
			A.[UpdateAmount],
			A.[GameTime],
			--A.[GameID],
			A.[rowid],
			B.ServerName,
			C.GameName,
			D.LoginName,
			D.NickName,
			D.GradeID,
			[PlatformName] = F.Name,
			A.[CurrentCouponAmount],
			A.[CouponAmount]
	FROM @Temp_PerGameLogDetail AS A		
		INNER JOIN ServerInfo.GameServerList AS B WITH (NOLOCK) ON A.ServerID = B.ServerID
		INNER JOIN ServerInfo.GameList AS C WITH (NOLOCK) ON B.GameID = C.GameID
		INNER JOIN Users.UserInfo AS D WITH (NOLOCK) ON A.UserID = D.UserID
		INNER JOIN Area.Area AS E WITH (NOLOCK) ON D.AreaID = E.AreaID
		INNER JOIN Area.Platform AS F WITH (NOLOCK) ON E.PlatformID = F.PlatformID
	ORDER BY A.rowid ASC
	-----------------------------------------------------------------------------------------------------	
END
