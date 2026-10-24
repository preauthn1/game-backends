


-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 充值贈送免費入場券
--	每15秒計算一次
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Event_ContestCalcBonus]
(
 @intGameID int
)
AS
BEGIN
SET NOCOUNT ON

--充值贈送
Declare @intUserID int
Declare @lngAmount bigint
Declare @intContestID int
Declare @chvContestName varchar(50)
Declare @dtEnterTime time
Declare @dtNow datetime=GETDATE()
Declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
Declare @dtStartTime datetime, @dtEndTime datetime
Declare @intMaxFreeTicket int
Declare @tnyIsFull tinyint=0 --額度是否已滿


DECLARE curEV CURSOR FOR

	--取得此遊戲正在比賽的場次
	select ID,StartTime,EndTime,Entertime,Name from Event.Contest where GameID=@intGameID and @dtNow >=StartTime and @dtNow < EndTime and Status=1
	
OPEN curEV
FETCH NEXT FROM curEV INTO @intContestID,@dtStartTime,@dtEndTime,@dtEnterTime,@chvContestName

WHILE @@FETCH_STATUS = 0
BEGIN

				--取得此場次最多的免費數量
				select @intMaxFreeTicket=Max(FreeTicket) from Event.Bonus where ContestID=@intContestID

				
				if OBJECT_ID('tempdb.dbo.#Order') is not null begin DROP TABLE #Order end
				
				CREATE TABLE #Order(UserID int,Amount bigint)

				--取得活動區間內的充值总额
				Insert #Order(UserID,Amount)
				Select UserID, ISNULL(Sum(Amount),0) as Amount from [Event].[ContestUserOrderLog] with (nolock) 
				where CreateTime>=@dtStartTime and CreateTime<@dtEndTime 
				Group by UserID

				DECLARE cur CURSOR FOR

					--计算用戶在活動區間內充值總和的免费券
					SELECT UserID,Amount from #Order

				OPEN cur
				FETCH NEXT FROM cur INTO @intUserID,@lngAmount

				WHILE @@FETCH_STATUS = 0
				BEGIN


					--取得今日已經領到的免費券數量
					Declare @intTodayGetFreeTicket int=0
					Select @intTodayGetFreeTicket=ISNULL(count(id),0) from Event.Ticket where UserID=@intUserID and ContestID=@intContestID and CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD

					--根據已達充值金額決定可贈送最大數量
					Declare @intFreeTicket int=0
					select top 1 @intFreeTicket=ISNULL(FreeTicket,0) from Event.Bonus where ContestID=@intContestID and @lngAmount >= OrderReach Order By OrderReach DESC

					--計算可贈送數量
					Declare @intTodayRemainFreeTicket int=0
					SET @intTodayRemainFreeTicket = @intFreeTicket-@intTodayGetFreeTicket
					IF @intTodayRemainFreeTicket < 0
						SET @intTodayRemainFreeTicket = 0

					--判斷是否已經額滿
					if @intTodayGetFreeTicket=@intMaxFreeTicket Set @tnyIsFull=1 ELSE Set @tnyIsFull=0

					print 'dtStartTime=' + convert(varchar,@dtStartTime)
					print 'dtEndTime=' + convert(varchar,@dtEndTime)
					print 'intContestID=' + convert(varchar,@intContestID)
					print 'intUserID=' + convert(varchar,@intUserID)
					print 'lngAmount=' + convert(varchar,@lngAmount)
					print 'tnyIsFull=' + convert(varchar,@tnyIsFull)
					print '此場次最多的免費數量 intMaxFreeTicket=' + convert(varchar,@intMaxFreeTicket)
					print '已達充值金額可贈送數量 intFreeTicket=' + convert(varchar,@intFreeTicket)
					print '今日已經領到數量 intTodayGetFreeTicket=' + convert(varchar,@intTodayGetFreeTicket)
					print '可贈送數量 intTodayRemainFreeTicket=' + convert(varchar,@intTodayRemainFreeTicket)
					print '----------------------------'


					--紀錄今日領取紀錄
					update Event.TicketJobLog set SumFreeTicket=SumFreeTicket+@intTodayRemainFreeTicket, IsFull=@tnyIsFull 
					where ContestID=@intContestID and UserID=@intUserID and YYYYMMDD=@intYYYYMMDD
					IF @@ROWCOUNT=0
						Insert Event.TicketJobLog(YYYYMMDD,GameID,ContestID,UserID,SumFreeTicket,isFull) values(@intYYYYMMDD, @intGameID, @intContestID, @intUserID, @intTodayRemainFreeTicket, @tnyIsFull)

					while(@intTodayRemainFreeTicket>0)
					begin
						--發送免費券
						Insert into Event.Ticket(ContestID,UserID,CreateTime,Type) values(@intContestID,@intUserID,@dtNow,1)
						--log
						insert into Event.ContestEventLog(UserID,Data) values(@intUserID,'比赛活动=' + @chvContestName + ', 充值金额=' + convert(varchar,@lngAmount) + ', 發送免费券 +1')

						Set @intTodayRemainFreeTicket -= 1
					end

				FETCH NEXT FROM cur INTO @intUserID,@lngAmount
				END
				CLOSE cur
				DEALLOCATE cur




FETCH NEXT FROM curEV INTO @intContestID,@dtStartTime,@dtEndTime,@dtEnterTime,@chvContestName
END
CLOSE curEV
DEALLOCATE curEV







END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Event_ContestCalcBonus] TO [xbuser]
    AS [dbo];

