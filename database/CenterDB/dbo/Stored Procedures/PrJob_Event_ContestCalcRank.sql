

-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 比賽每10分鐘計算一次排行榜
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Event_ContestCalcRank]
AS
BEGIN

	SET NOCOUNT ON
	declare @dtNow datetime=GETDATE()
	declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	declare @intContestID int
	declare @tnyType tinyint

	declare @dtEntertime time	--日結算: 進場時間
	declare @dtDeadline time	--日結算: 截止時間
	declare @dtRankBeginTime datetime --日結算: 算分開始時間
	declare @dtRankLastTime datetime --日結算: 算分結束時間 (當日截止時間+延遲計算獎勵分鐘)

	declare @dtPeriodEntertime time --週期結算: 進場時間
	declare @dtPeriodDeadline time  --週期結算: 進場時間
	declare @dtRankPeriodBeginTime datetime --週期結算: 算分開始時間
	declare @dtRankPeriodLastTime datetime --週期結算: 算分結束時間 (區間時間點截止時間+延遲計算獎勵分鐘)
	declare @intPeriod int --週期結算: 第幾個週期

	declare @tnyLastCalcRank tinyint=0 --是否是最後一次結算
	declare @dtStartTime datetime --活動開始時間
	declare @dtEndTime datetime --活動結束時間

	declare @intRewardAfterMin int
	declare @intRankInterval int
	declare @dtUpdateTime datetime


	DECLARE cur CURSOR FOR
	
		--找出正在進行的場次 (開始時間 ~ 最後一天截止時間dealline + 延遲計算獎勵分鐘 RewardAfterMin + 過20分內)
		Select 
		ID, 
		StartTime,
		EndTime,
		Entertime,
		Deadline,
		PeriodEntertime,
		PeriodDeadline,
		RewardAfterMin,
		RankInterval

		from Event.Contest with (nolock) where
		StartTime <= @dtNow AND @dtNow <= DateAdd(MINUTE,RewardAfterMin,EndTime)

		--DateDiff(MINUTE,
		--	--deadline time + after min
		--	DateAdd(MINUTE,RewardAfterMin,
		--		cast(cast(EndTime as date) as Datetime) + cast(deadline as DateTime) --lastdate date + deadline time
		--	) 
		--,@dtNow)  <= 20
		
	OPEN cur
	FETCH NEXT FROM cur INTO @intContestID, @dtStartTime, @dtEndTime, @dtEntertime, @dtDeadline, @dtPeriodEntertime, @dtPeriodDeadline, @intRewardAfterMin,@intRankInterval

	WHILE @@FETCH_STATUS = 0
	BEGIN

	
		--==================================================================================================================
		--每日結算
		set @tnyType=1

		--如果有最後一次結算紀錄, 則不進行排行結算
		if NOT EXISTS(select 1 from Event.Ranking where ContestID=@intContestID and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD and LastCalcRank=1 and Type=@tnyType)
		begin

			--開始時間 目前時間+入場時間
			set @dtRankBeginTime = cast(cast(@dtNow as date) as Datetime) + cast(@dtEntertime as datetime)

			--目前時間+截止時間+延遲計算時間
			set @dtRankLastTime=
			DateAdd(MINUTE,@intRewardAfterMin,
				cast(cast(@dtNow as date) as Datetime) + cast(@dtDeadline as DateTime) --lastdate date + deadline time
			)

			--如果已經過了最後時間才統計, 就用結束時間紀錄
			if @dtNow > @dtRankLastTime set @dtUpdateTime=@dtRankLastTime ELSE set @dtUpdateTime=@dtNow
			
			--是否是當日最後一次結算
			if @dtNow >= @dtRankLastTime Set @tnyLastCalcRank = 1 ELSE  Set @tnyLastCalcRank = 0

			--如果是最後一次結算,塞入機器人分數
			if @tnyLastCalcRank = 1 
			begin 
				print 'robot skip'
				--exec PrJob_Event_AddRobotScore @intContestID
			end 

			print 'daily:'
			print '@intContestID=' + convert(varchar,@intContestID)
			print '@tnyType=' + convert(varchar,@tnyType)
			print '@tnyLastCalcRank=' + convert(varchar,@tnyLastCalcRank)
			print '@dtUpdateTime=' + convert(varchar,@dtUpdateTime)
			print '@dtRankBeginTime=' + convert(varchar,@dtRankBeginTime)
			print '@dtRankLastTime=' + convert(varchar,@dtRankLastTime)

			--更新排行榜
			Merge into Event.Ranking as R
			Using(
				--計算排行
				select @intContestID as ContestID,UserID,Score,DENSE_RANK() OVER(ORDER BY Score DESC) AS [Rank] from (
					select UserID,Max(score) as score from Event.Score 
					where ContestID=@intContestID 
					--and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD 
					and CreateTime >= @dtRankBeginTime
					and CreateTime < @dtRankLastTime
					group by UserID	
				) A
			) as S
			ON 
			R.ContestID=S.ContestID --條件:該場次 
			AND R.UserID=S.UserID --條件:該玩家 
			AND	CAST(CONVERT(varchar,R.CreateTime,112) AS INT)=@intYYYYMMDD --更新當天
			AND R.Type=@tnyType --日結算類型

			WHEN MATCHED THEN
				UPDATE SET 
				R.Score=S.Score, R.Rank=S.Rank, 
				R.CreateTime=@dtUpdateTime, --更新分數與結算時間
				R.LastCalcRank=@tnyLastCalcRank
			WHEN NOT MATCHED THEN
				INSERT (ContestID,UserID,Score,CreateTime,[Rank],Type,LastCalcRank,Reward)
				Values(S.ContestID,S.UserID,S.Score,@dtUpdateTime,S.Rank,@tnyType,@tnyLastCalcRank,null);

		end


		--==================================================================================================================
		--區間結算
		declare @intDayDiff int
		set @tnyType=2

		--取得區間天數
		select @intDayDiff=datediff(day,@dtStartTime,@dtNow) + 1
		--如果在區間點或最後一天,進行週期結算, 且區間點不能是一天(跟日結算重複)
		--if @intDayDiff % @intRankInterval = 0 and @intRankInterval > 1
		if  @intRankInterval > 1
		begin


			--週期開始時間(今天減去週期(interval) + 週期入場時間
			--set @dtRankPeriodBegintime = cast(cast( dateadd(day,(-@intRankInterval)+1,@dtNow) as date) as Datetime) + cast(@dtPeriodEntertime as datetime)
	

			--週期開始時間(第一天 + 週期入場時間
			declare @intRemainder int
			set @intRemainder = @intDayDiff%@intRankInterval --餘數

			if @intRemainder=0 -- 累積賽區段最末天
			begin
				set @intRemainder = @intRankInterval-1
				--開始時間
				set @dtRankPeriodBegintime = cast(cast( DATEADD(day,-@intRemainder,@dtNow) as date) as datetime) + cast(@dtPeriodEntertime as datetime)
				--目前時間+截止時間+延遲計算時間
				set @dtRankPeriodLastTime=
				DateAdd(MINUTE,@intRewardAfterMin,
					cast(cast(@dtNow as date) as Datetime) + cast(@dtPeriodDeadline as DateTime) --lastdate date + deadline time
				)
				--計算第幾個週期
				set @intPeriod=@intDayDiff/@intRankInterval
				--是否是最後一天最後一次結算
				if @dtNow >= @dtRankPeriodLastTime set @tnyLastCalcRank = 1
				--結算更新時間
				if @dtNow > @dtRankPeriodLastTime set @dtUpdateTime=@dtRankPeriodLastTime
			end
			else
			begin
				set @intRemainder = @intRemainder-1
				--開始時間
				set @dtRankPeriodBegintime = cast(cast( DATEADD(day,-@intRemainder,@dtNow) as date) as datetime) + cast(@dtPeriodEntertime as datetime)
				--目前時間
				set @dtRankPeriodLastTime=@dtNow
				--計算第幾個週期
				set @intPeriod=@intDayDiff/@intRankInterval + 1
				--是否是最後一天最後一次結算
				set @tnyLastCalcRank = 0
				--結算更新時間
				set @dtUpdateTime=@dtNow
			end
			


			print 'Period:'
			print '@intContestID=' + convert(varchar,@intContestID)
			print '@tnyType=' + convert(varchar,@tnyType)
			print '@intDayDiff=' + convert(varchar,@intDayDiff)
			print '@intPeriod=' + convert(varchar,@intPeriod)
			print '@tnyLastCalcRank=' + convert(varchar,@tnyLastCalcRank)
			print '@dtUpdateTime=' + convert(varchar,@dtUpdateTime)
			print '@dtRankPeriodBegintime=' + convert(varchar,@dtRankPeriodBegintime)
			print '@dtRankPeriodLastTime=' + convert(varchar,@dtRankPeriodLastTime)


			--如果有最後一次結算紀錄, 則不進行排行結算
			if NOT EXISTS(select 1 from Event.Ranking where ContestID=@intContestID and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD and LastCalcRank=1 and Type=@tnyType)
			begin
				--更新排行榜
				Merge into Event.Ranking as R
				Using(
					--計算排行
					select @intContestID as ContestID,UserID,Score,DENSE_RANK() OVER(ORDER BY Score DESC) AS [Rank] from (
						select UserID,MAX(score) as score from Event.Score
						where ContestID=@intContestID 
						and CreateTime >= @dtRankPeriodBegintime
						and CreateTime < @dtRankPeriodLastTime
						--and CAST(CONVERT(varchar,CreateTime,112) AS INT) >= CAST(CONVERT(varchar, dateadd(day,(-@intRankInterval)+1,@dtNow), 112) AS INT) ----結算區間開始
						--and CAST(CONVERT(varchar,CreateTime,112) AS INT) <= @intYYYYMMDD --結算今天以前
						group by UserID
					) A

				) as S
				ON 
				R.ContestID=S.ContestID 
				AND R.UserID=S.UserID --條件:該場次該玩家
				--AND	CAST(CONVERT(varchar,R.CreateTime,112) AS INT)=@intYYYYMMDD --更新當天
				AND R.Type=@tnyType --區間結算類型
				AND R.Period = @intPeriod 
				WHEN MATCHED THEN
					UPDATE SET 
					R.Score=S.Score, R.Rank=S.Rank, 
					R.CreateTime=@dtUpdateTime, --更新分數與結算時間
					R.LastCalcRank=@tnyLastCalcRank
				WHEN NOT MATCHED THEN
					INSERT (ContestID,UserID,Score,CreateTime,[Rank],Type,LastCalcRank,Reward,Period)
					Values(S.ContestID,S.UserID,S.Score,@dtUpdateTime,S.Rank,@tnyType,@tnyLastCalcRank,null,@intPeriod);
			end

		end



	FETCH NEXT FROM cur INTO @intContestID, @dtStartTime, @dtEndTime, @dtEntertime, @dtDeadline, @dtPeriodEntertime, @dtPeriodDeadline, @intRewardAfterMin,@intRankInterval
	END

	CLOSE cur
	DEALLOCATE cur



END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Event_ContestCalcRank] TO [xbuser]
    AS [dbo];

