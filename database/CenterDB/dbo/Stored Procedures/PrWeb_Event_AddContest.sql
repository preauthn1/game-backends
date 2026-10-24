

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 新增/更新比赛场次资讯
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_AddContest]
(
	@intContestID int=null, --場次ID
	@chvName varchar(50), --場次名稱
	@intGameID int,
	--@tnyType tinyint, --日賽(1)或週賽(2)
	@dtStartTime datetime, --開始時間
	@dtEndTime datetime, --結束時間 (不能早於入場截止時間)
	@tnyStatus	tinyint, --open 1 close 0
	@tnyShow tinyint, --是否顯示
	@dtEnterTime	time, --入場時間
	@dtDeadline	time = '23:00:00', --日結算: 截止時間
	@dtPeriodEnterTime	time, --週期結算: 入場時間
	@dtPeriodDeadline	time = '23:00:00', --週期結算: 截止時間
	@intRewardAfterMin int, --結束後過幾分鐘計算獎勵
	@intRankInterval int,--區間計算週期(day)
	@lngCharge bigint, --入場費
	@intPlayTime int, --遊戲時間
	@lngCoin bigint, --比賽幣
	@intRtnContestID int = 0 OUTPUT, --回傳ID
	@chvRtnMsg	varchar(50) = '' OUTPUT, --錯誤訊息
	@intRtnCode int OUTPUT --錯誤代碼
)
AS
BEGIN

	SET NOCOUNT ON	

	IF @intRankInterval = 1
	BEGIN
		SET @chvRtnMsg='累积赛结算天数需大于一天'
		SET @intRtnCode=-1
		Return
	END

	declare @intDayDiff int
	select @intDayDiff=datediff(day,@dtStartTime,@dtEndTime) + 1
	IF @intRankInterval>@intDayDiff
	BEGIN
		SET @chvRtnMsg='累积赛结算天数不能大于活动天数'
		SET @intRtnCode=-1
		Return
	END

	--需要檢查deadline+@intRewardAfterMin時間不能超過晚上11:30 避免當日統計獎勵計算超過整點問題
	declare @dtLimitTime time = '23:30:00'
	IF dateadd(MINUTE,@intRewardAfterMin,@dtDeadline) >  @dtLimitTime
	BEGIN
		SET @chvRtnMsg='奖励计算时间(截止时间+结束后几分钟结算)不能超过23:30'
		SET @intRtnCode=-1
		Return
	END


	IF dateadd(MINUTE,@intRewardAfterMin,@dtPeriodDeadline) >  @dtLimitTime 
	BEGIN
		SET @chvRtnMsg='累积赛奖励计算时间(累积赛截止时间+结束后几分钟结算)不能超过23:30'
		SET @intRtnCode=-1
		Return
	END


	--检查场次时间是否重叠
	IF EXISTS(
		Select 1 from Event.Contest where 
		GameID=@intGameID 
		and (@intContestID is null or ID <> @intContestID) --如果是新增資料ID=null, 如果是更新資料檢查其它ID的時間是否重疊
		and 
		(   --該場次結束時間是否落在其他場次的開始與結束區段
		 ( CAST(CONVERT(varchar,@dtEndTime,112) AS INT)  >= CAST(CONVERT(varchar,StartTime,112) AS INT) AND CAST(CONVERT(varchar,@dtEndTime,112) AS INT)  <= CAST(CONVERT(varchar,EndTime,112) AS INT) )
		 OR --該場次開始時間是否落在其他場次的開始與結束區段
		 ( CAST(CONVERT(varchar,@dtStartTime,112) AS INT)  >= CAST(CONVERT(varchar,StartTime,112) AS INT) AND CAST(CONVERT(varchar,@dtStartTime,112) AS INT)  <= CAST(CONVERT(varchar,EndTime,112) AS INT) )
		 OR --該場次開始時間是否在其他場次之前, 結束時間是否在其他場次之後
		 ( CAST(CONVERT(varchar,@dtStartTime,112) AS INT)  <= CAST(CONVERT(varchar,StartTime,112) AS INT) AND CAST(CONVERT(varchar,@dtEndTime,112) AS INT)  >= CAST(CONVERT(varchar,EndTime,112) AS INT) )
		) 
	)
	BEGIN
		SET @chvRtnMsg='比赛场次时间设定已经重叠,请重新设定'
		SET @intRtnCode=-1
		Return
	END

	IF Cast(@dtEndTime as Time) < @dtDeadline
	BEGIN
		SET @chvRtnMsg='活动结束时间不能早于截止时间'
		SET @intRtnCode=-1
		Return
	END

	IF  Cast(@dtEndTime as Time) < @dtPeriodDeadline
	BEGIN
		SET @chvRtnMsg='活动结束时间不能早于累积赛截止时间'
		SET @intRtnCode=-1
		Return
	END


	IF @dtEndTime < @dtStartTime 
	BEGIN
		SET @chvRtnMsg='活动结束时间不能早于開始时间'
		SET @intRtnCode=-1
		Return
	END



	IF @intContestID is NULL
	BEGIN
		Insert into Event.Contest(Name,Gameid,StartTime,EndTime,Status,Show,EnterTime,Deadline,PeriodEntertime, PeriodDeadline,RewardAftermin,RankInterval,Charge,PlayTime,Coin)
		values(@chvName,@intGameID,@dtStartTime,@dtEndTime,@tnyStatus,@tnyShow,@dtEnterTime,@dtDeadline,@dtPeriodEnterTime, @dtPeriodDeadline,@intRewardAfterMin,@intRankInterval,@lngCharge,@intPlayTime,@lngCoin)
		SET @intRtnContestID=SCOPE_IDENTITY()

	END
	ELSE
	BEGIN
		Update Event.Contest
			Set
			Name=@chvName,
			Gameid=@intGameID,
			StartTime=@dtStartTime,
			EndTime=@dtEndTime,
			Status=@tnyStatus,
			Show=@tnyShow,
			Entertime=@dtEnterTime,
			Deadline=@dtDeadline,
			PeriodEntertime=@dtPeriodEnterTime,
			PeriodDeadline=@dtPeriodDeadline,
			RewardAfterMin=@intRewardAfterMin,
			RankInterval=@intRankInterval,
			Charge=@lngCharge,
			PlayTime=@intPlayTime,
			Coin=@lngCoin
			Where ID=@intContestID

		SET @intRtnContestID=@intContestID
	
	END


	SET @chvRtnMsg='操作成功'
	SET @intRtnCode=1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_AddContest] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_AddContest] TO [xbuser]
    AS [dbo];

