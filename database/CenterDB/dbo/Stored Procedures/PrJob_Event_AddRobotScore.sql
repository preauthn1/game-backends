


-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 新增機器人至排行榜
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Event_AddRobotScore]
(
@intContestID int
)
AS
BEGIN

	SET NOCOUNT ON

	declare @tnyType tinyint
	declare @dtNow datetime=getdate()
	declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	declare @intPlayerNumber int
	declare @intMinScore int --最高分數
	declare @intMaxScore int --最低分數
	declare @intRobotPercentage int=30
	declare @intExpectRobotAmount int =0 --= Cast(RAND()*(@intMaxRobotAmount-1)+1 as int)
	declare @intCurrentRobotAmount int =0
	declare @intAddRobotAmount int=0
	
	
	--檢查每天是否有機器人
	select @intMaxScore=ISNULL(Max(Score),0),@intMinScore=ISNULL(Min(Score),0),
	@intPlayerNumber=Count(distinct CASE WHEN Robot=0 THEN UserID END), 
	@intCurrentRobotAmount=Count(distinct CASE WHEN Robot=1 THEN UserID END)
	from Event.Score with(nolock) where ContestID=@intContestID and @intYYYYMMDD=CAST(CONVERT(varchar,CreateTime,112) AS INT)

	--set @intPlayerNumber = 20
	--set @intCurrentRobotAmount=5

	IF @intPlayerNumber<10
		SET @intRobotPercentage=200
	ELSE
	IF @intPlayerNumber<20
		SET @intRobotPercentage=100
	ELSE
		SET @intRobotPercentage=30

	set @intExpectRobotAmount = @intPlayerNumber * @intRobotPercentage/100
	
	set @intAddRobotAmount = @intExpectRobotAmount-@intCurrentRobotAmount
	IF @intAddRobotAmount<0
		set @intAddRobotAmount=0

	print 'player=' + convert(varchar, @intPlayerNumber)
	print 'robot percentage=' + convert(varchar,@intRobotPercentage)
	print 'expect robot=' + convert(varchar, @intExpectRobotAmount)
	print 'current robot=' + convert(varchar, @intCurrentRobotAmount)
	print 'add robot=' + convert(varchar, @intAddRobotAmount)
	
	--塞入機器人
	Insert into Event.Score(ContestID,UserID,Score,Robot)
	select top (@intAddRobotAmount) 
	@intContestID as ContestID, 
	UserID,
	ABS(CHECKSUM(NewId())) % @intMaxScore + @intMinScore as score,
	--round(rand(checksum(newid()))*( (@intMaxScore-@intMinScore ) )+@intMinScore,0),
	1 as Robot
	--@intMaxScore as Maxscore,@intMinScore as Minscore
	from Robot.RobotList
	order by newid()


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Event_AddRobotScore] TO [xbuser]
    AS [dbo];

