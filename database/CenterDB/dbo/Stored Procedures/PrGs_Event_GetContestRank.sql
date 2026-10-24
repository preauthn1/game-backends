


-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 取得比賽排行
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_GetContestRank]
(
@intContestID int
)
AS
BEGIN

SET NOCOUNT ON
declare @dtNow datetime=getdate()
declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
declare @intYesYYYYMMDD int=CAST(CONVERT(varchar,@dtNow-1,112) AS INT)
declare @dtStartTime datetime 
declare @intDayDiff int
declare @intRankInterval int
declare @intPeriod int


--日排行
select top 10 @intYYYYMMDD as YYYYMMDD,UserID as ShowUserID, SUBSTRING(Convert(varchar, UserID),1,4) + REPLICATE('X',4) as UserID, Rank,Score from Event.Ranking with(nolock)
where ContestID=@intContestID and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD and Type=1
order by Rank asc

--昨日排行
select top 10 @intYesYYYYMMDD as YYYYMMDD,UserID as ShowUserID, SUBSTRING(Convert(varchar, UserID),1,4) + REPLICATE('X',4) as UserID,Rank,Score from Event.Ranking with(nolock)
where ContestID=@intContestID and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYesYYYYMMDD and Type=1
order by Rank asc



--累積賽排行
select @dtStartTime=StartTime,@intRankInterval=RankInterval from Event.Contest where ID=@intContestID
select @intDayDiff=datediff(day,@dtStartTime,@dtNow) + 1

declare @intRemainder int
set @intRemainder = @intDayDiff%@intRankInterval --餘數
if @intRemainder=0 -- 累積賽區段最末天
	set @intPeriod=@intDayDiff/@intRankInterval --商數=週期
else
	set @intPeriod=@intDayDiff/@intRankInterval + 1

select top 10 @intYYYYMMDD as YYYYMMDD,UserID as ShowUserID, SUBSTRING(Convert(varchar, UserID),1,4) + REPLICATE('X',4) as UserID,Rank,Score from Event.Ranking with(nolock)
where ContestID=@intContestID and Type=2 and Period=@intPeriod
order by Rank asc

RETURN 1

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestRank] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_GetContestRank] TO [xbuser]
    AS [dbo];

