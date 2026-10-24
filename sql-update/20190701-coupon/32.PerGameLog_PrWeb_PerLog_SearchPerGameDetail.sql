USE [PerGameLog]
GO
/****** Object:  StoredProcedure [dbo].[PrWeb_PerLog_SearchPerGameDetail]    Script Date: 2019/7/1 下午 07:11:56 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<xxx>
-- Create date: <xxx>
-- Description:	<查询每局游戏记录>
--	2018/03/26	Jason		新增d.GradeID as 'GradeID',
--	2018/04/13	Jason		取消輸入參數@chvLogId VARCHAR(256)   一律使用@longDealNo
--	2018/04/16	Jason		調整語法及結構 取消@chvDeskCode
--  2019/07/01  Rory        禮碼list
-- =============================================
ALTER PROCEDURE [dbo].[PrWeb_PerLog_SearchPerGameDetail]
	@intUserID		INT = NULL,			--用户ID
	@intGameID		INT = NULL,			--游戏ID
	@intServerID	INT = NULL,			--服务器ID
	@longDealNo     BIGINT = NULL,		--牌局号 --牌局标识
	@chvDeskCode	VARCHAR(15) = NULL,	--房间口令
	@dtBeginTime	DATETIME = NULL,	--开始时间范围
	@dtEndTime		DATETIME = NULL,	--结束时间范围
	@intPageIndex	int = 1,
	@intPageSize    int = 20,
	@intPageCount   int			OUTPUT
AS
BEGIN

	SET NOCOUNT ON
	
	DECLARE	@StartDays	INT
	DECLARE @EndDays	INT
	
	--如果牌局号為空, 強制設定查詢區間
	--if @longDealNo is NULL 
	--Begin

		--如果沒有帶任何值, 預設查詢1小時範圍
		IF @dtBeginTime IS NULL AND  @dtEndTime IS NULL
		BEGIN
			SET @dtEndTime = GETDATE()
			SET @dtBeginTime = DATEADD(HOUR,-1,@dtEndTime)
		END
		ELSE
		--如果開始時間沒有帶任何值,結束時間有帶值,預設查詢1小時範圍
		IF @dtBeginTime IS NULL AND @dtEndTime IS NOT NULL
		BEGIN
			SET @dtBeginTime = dateadd(HOUR,-1,@dtEndTime)
		END
		--如果結束時間沒有帶值,預設查詢1小時範圍
		ELSE IF @dtEndTime IS NULL
		BEGIN
			SET @dtEndTime = dateadd(HOUR,1,@dtBeginTime)
		END
	--END

	SET @StartDays = DATEDIFF(MINUTE,0,@dtBeginTime)
	SET @EndDays = DATEDIFF(MINUTE,0,@dtEndTime)	

	/*  暫時使用  */
	-----------------------------------------------------------------------------------------------------
	DECLARE @isFindServerID BIT 
	SET @isFindServerID = 1
	
	CREATE TABLE #TempServerID(ServerID INT NOT NULL)		
	IF @intServerID IS NULL AND @intGameID IS NOT NULL
	BEGIN
		INSERT INTO #TempServerID	
		SELECT ServerID 	
		FROM CENTERDBLINKEDSERVER.[Centerdb].ServerInfo.GameServerList WHERE GameID = @intGameID	
	END
	ELSE IF @intServerID IS NOT NULL
	BEGIN
		INSERT INTO #TempServerID 
		SELECT @intServerID
		FROM CENTERDBLINKEDSERVER.[Centerdb].ServerInfo.GameServerList WHERE ServerID = @intServerID	
	END
	
	SET @isFindServerID = CASE WHEN EXISTS(SELECT 1 FROM #TempServerID) THEN 1 ELSE 0 END
	-----------------------------------------------------------------------------------------------------

	
	SELECT  @intPageCount = COUNT(a.id) 
	FROM PerLog.DetailLog as a with(NOLOCK)		 
	WHERE
		(a.[days] >= @StartDays AND a.[days] <= @EndDays)
		--AND (ISNULL(@intGameID,a.GameID) = a.GameID)
		--AND (ISNULL(@intServerID,a.ServerID) = a.ServerID)
		AND (@isFindServerID = 0 OR a.ServerID IN(SELECT ServerID FROM #TempServerID))
		AND (ISNULL(@intUserID,a.UserID) = a.UserID)
		AND (ISNULL(@longDealNo,a.LogID) = a.LogID)


	;with tb as (		
			SELECT a.[Id],a.[LogID],a.[ServerID],a.[UserID],a.[CurrentAmount],a.[UpdateAmount],a.[GameTime],a.[CurrentCouponAmount],a.[CouponAmount],
					ROW_NUMBER() over(order by a.Id desc) as rowid
					--ROW_NUMBER() over(order by a.[days] desc,a.Id desc) as rowid
			FROM PerLog.DetailLog as  a with(NOLOCK) 
			WHERE
				(a.[days] >= @StartDays AND a.[days] <= @EndDays)
				--AND (ISNULL(@intGameID,a.GameID) = a.GameID)
				--AND (ISNULL(@intServerID,a.ServerID) = a.ServerID)
				AND (@isFindServerID = 0 OR a.ServerID IN(SELECT ServerID FROM #TempServerID))
				AND (ISNULL(@intUserID,a.UserID) = a.UserID)
				AND (ISNULL(@longDealNo,a.LogID) = a.LogID)
	), 
	result 
	AS 
	(
		SELECT * 
		FROM tb 
		WHERE rowid between @intPageSize * (@intPageIndex-1) + 1  and @intPageSize * @intPageIndex
	)


	SELECT	result.*,
			b.ServerName as 'ServerName',
			c.GameName as 'GameName',
			d.LoginName as 'LoginName',
			d.NickName as 'NickName',
			d.GradeID as 'GradeID',
			f.Name as 'PlatformName'
	FROM result		
		INNER JOIN CENTERDBLINKEDSERVER.[Centerdb].ServerInfo.GameServerList as b on result.ServerID = b.ServerID
		INNER JOIN CENTERDBLINKEDSERVER.[Centerdb].ServerInfo.GameList as c on b.GameID = c.GameID
		INNER JOIN CENTERDBLINKEDSERVER.[Centerdb].Users.UserInfo as d on result.UserID = d.UserID
		INNER JOIN CENTERDBLINKEDSERVER.[Centerdb].Area.Area as e on d.AreaID=e.AreaID
		INNER JOIN CENTERDBLINKEDSERVER.[Centerdb].Area.Platform as f on e.PlatformID=f.PlatformID
	order by result.rowid asc
		
END
