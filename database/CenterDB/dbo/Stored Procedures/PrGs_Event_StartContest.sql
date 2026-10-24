
-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 參加活動, 扣金幣 MoneySourceType=21002
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_Event_StartContest]
(
	@intContestID int, --場次ID
	@intContestNo int, --該場次的第幾局(用來判斷要不要扣錢/免費次數)
	@intGameID int, --遊戲類型
	@intUserID int,
	@chvErrMsg	varchar(50) = '' OUTPUT, --錯誤訊息
	@intRtnCode int OUTPUT --錯誤代碼
)
AS
BEGIN

	SET NOCOUNT ON
	Declare @chvGameName varchar(20)
	Declare @dtNow datetime=GETDATE()
	Declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	Declare @lngCharge bigint --入場費(單位:分)
	Declare @dtEnterTime Time, @dtDeadline Time
	Declare @dtStartTime datetime, @dtEndTime datetime
	Declare @chvContestName varchar(50)
	Declare @intLastContestNo int=0 --最後一次扣錢/扣免費次數的局號

	--取得遊戲名稱
	select @chvGameName=GameName from ServerInfo.GameList where GameID=@intGameID

	--取得開放的比賽場次
	select @dtStartTime=StartTime,@dtEndTime=EndTime,@dtEnterTime=Entertime,@dtDeadline=Deadline,@lngCharge=Charge,@chvContestName=Name
	from Event.Contest with (nolock)
	where GameID=@intGameID and @dtNow >=StartTime and @dtNow < EndTime and Status=1 and ID=@intContestID
	IF @@ROWCOUNT=0
	Begin
		SET @chvErrMsg='目前没有开放比赛场次'
		SET @intRtnCode=-1
		RETURN @intRtnCode
	End

	--檢查入場限制時間
	IF CAST(getdate() AS time) < @dtEnterTime 
	Begin
		SET @chvErrMsg='尚未开放入场'
		SET @intRtnCode=-1
		RETURN @intRtnCode
	End	

	IF CAST(getdate() AS time) > @dtDeadline 
	Begin
		SET @chvErrMsg='已经超过入场截止时间'
		SET @intRtnCode=-1
		RETURN @intRtnCode
	End	


	IF NOT EXISTS (select 1 from Users.UserInfo WITH(NOLOCK) where UserID=@intUserID)
	Begin
		SET @chvErrMsg='帐号不存在'
		SET @intRtnCode=-1
		RETURN @intRtnCode
	End	


	IF @intContestNo=0
	Begin
		SET @chvErrMsg='场次编号 ContestNo 错误'
		SET @intRtnCode=-1
		RETURN @intRtnCode
	End	

	--最後一次付費的局號
	select @intLastContestNo=ContestNo from Event.ContestUserData with (nolock) where ContestID=@intContestID and GameID=@intGameID and UserID=@intUserID
	if @@ROWCOUNT=0
	Begin

		--把之前舊的清除
		delete Event.ContestUserData where GameID=@intGameID and UserID=@intUserID

		insert into Event.ContestUserData(GameID,UserID,ContestID,ContestNo,FlagID,TableNo,PlayTime,RemainScore,Score,CreateTime)
		values(@intGameID,@intUserID,@intContestID,@intContestNo,0,0,99999,0,0,GETDATE())
		SET @intLastContestNo=0
	--	SET @chvErrMsg='帐号不存在'
	--	SET @intRtnCode=-1
	--	RETURN @intRtnCode
	End	


	--log
	insert into Event.ContestEventLog(UserID, Data) values(@intUserID,'比赛活动=' + @chvContestName + ', Start contest')


	--檢查今日是否還有未使用的免費券
	Declare @intFreeTicket int
	Declare @intTicketID int

	Select @intTicketID=Min(ID), @intFreeTicket=ISNULL(Count(ID),0) From Event.Ticket WITH (UPDLOCK)
	Where UserID=@intUserID AND ContestID=@intContestID 
	--不判斷時間了, 沒使用的也算進去  
	--AND CAST(CONVERT(varchar,CreateTime,112) AS INT)=@intYYYYMMDD 
	AND UpdateTime IS NULL



	--检查这局是否已经付费
	if @intContestNo <=@intLastContestNo 
	Begin
		--log
		insert into Event.ContestEventLog(userid,data) values(@intUserID,'比赛活动=' + @chvContestName + ', 免费券数量=' + convert(varchar,@intFreeTicket) + ', 已经付费, 不扣費/不扣免费券')

		SET @chvErrMsg=''
		SET @intRtnCode=1
		RETURN @intRtnCode
	End


	IF @intFreeTicket > 0
	BEGIN

		--扣免费券
		Update Event.Ticket set UpdateTime=Getdate() Where ID = @intTicketID
		Update Event.ContestUserData set ContestNo=@intContestNo,Playtime=99999 where GameID=@intGameID and ContestID=@intContestID and UserID=@intUserID
		--log
		insert into Event.ContestEventLog(userid,data) values(@intUserID,'比赛活动=' + @chvContestName + ', 免费券数量=' + convert(varchar,@intFreeTicket) + ', 扣免费券 -1,' + ' 剩馀数量=' + convert(varchar,@intFreeTicket-1))

	End
	ELSE
	BEGIN
		--如果沒有則扣金幣
		declare @chvIPAddress varchar(40)='0.0.0.0'
		declare @sintSourceType int = 21002
		declare @chvMemo varchar(128) = @chvGameName + ':' + @chvContestName + '比赛活动扣金币'
		declare @lngAmount bigint = @lngCharge*-1
		declare @lngCurrentAmount bigint
		declare @return_value int


		EXEC @intRtnCode = dbo.PrWeb_UpdateUserCash
			@intUserID = @intUserID,
			@sintSourceType = @sintSourceType,
			@lngUpdateAmount = @lngAmount,
			@chvIPAddress = @chvIPAddress,
			@chvMemo = @chvMemo,
			@lngCurrentAmount = @lngCurrentAmount OUTPUT,
			@chvErrMsg = @chvErrMsg OUTPUT	

		IF @intRtnCode <> 1
			RETURN @intRtnCode

		Update Event.ContestUserData set ContestNo=@intContestNo,Playtime=99999 where GameID=@intGameID and ContestID=@intContestID and UserID=@intUserID

		--log
		insert into Event.ContestEventLog(UserID,Data) values(@intUserID,'比赛活动=' + @chvContestName + ', 免费券数量=' + convert(varchar,@intFreeTicket) + ', 扣金幣 ' + convert(varchar,@lngAmount))

	END

	SET @chvErrMsg=''
	SET @intRtnCode=1
	RETURN @intRtnCode
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_StartContest] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_Event_StartContest] TO [xbuser]
    AS [dbo];

