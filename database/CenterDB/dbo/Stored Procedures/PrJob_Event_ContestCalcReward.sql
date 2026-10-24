


-- =============================================
-- Author:  alan
-- Create date: 20181211
-- Description: 比賽活動結束按排名計算贈送獎勵金
--	每10分鐘計算一次
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_Event_ContestCalcReward]
AS
BEGIN

	SET NOCOUNT ON
	declare @intRankID int
	declare @tnyType tinyint=0 -- 結算類型 日結算(1) 區間結算(2)
	declare @intGameID int
	declare @intContestID int=0
	declare @chvContestName varchar(50)=''
	declare @dtNow datetime=getdate()
	declare @intYYYYMMDD int=CAST(CONVERT(varchar,@dtNow,112) AS INT)
	declare @chvGameName varchar(20) --遊戲名稱
	declare @lngScore bigint
	declare @intRank tinyint --排名
	declare @intRewardUserID int --獎勵用戶ID
	declare @lngRewardMoney bigint --獎勵金額
	declare @chvIPAddress varchar(40)=''
	declare @tnyUserType tinyint
	declare @sintSourceType int = 21003
	declare @chvTitle varchar(20) = '活动奖励金发送成功'
	declare @chvMemo varchar(50) = '活动奖励金币'
	declare @chvMailContent varchar(500)
	declare @lngAmount bigint
	declare @chvErrMsg	VARCHAR(128)
	declare @lngCurrentAmount bigint
	declare @chvT varchar(200),@chvM varchar(200) --郵件title與註解


	DECLARE cur CURSOR FOR

		select ID, ContestID, UserID, Rank, [Type], Score from Event.Ranking where LastCalcRank=1 and  Reward is null
	

	OPEN cur
	FETCH NEXT FROM cur INTO @intRankID, @intContestID, @intRewardUserID, @intRank, @tnyType, @lngScore

	WHILE @@FETCH_STATUS = 0
	BEGIN

		--取得場次遊戲資訊
		select @intGameID=GameID,@chvContestName=[Name] from Event.Contest with (nolock) where ID=@intContestID
		--取得遊戲名稱
		select @chvGameName=GameName from ServerInfo.GameList with (nolock) where GameID=@intGameID

		--設定註解
		set @chvM=@chvGameName + ':' + @chvContestName + @chvMemo
		set @chvT=@chvGameName+@chvTitle

		--取得獎勵金幣设定
		set @lngRewardMoney=0
		select @lngRewardMoney=Money from Event.Reward where ContestID=@intContestID and Rank=@intRank and Type=@tnyType

		print '@intRankID=' + convert(varchar,@intRankID)
		print '@intRewardUserID=' + convert(varchar,@intRewardUserID)
		print '@intRank=' +convert(varchar,@intRank)
		print '@lngRewardMoney=' +convert(varchar,@lngRewardMoney)	

		--紀錄給獎勵內容
		insert Event.RewardJobLog(YYYYMMDD,Type,GameID,ContestID,UserID,Rank,Score,Reward) 
		values(@intYYYYMMDD,@tnyType,@intGameID,@intContestID,@intRewardUserID,@intRank,@lngScore,@lngRewardMoney)

		--取得用戶IP
		select @chvIPAddress=LastLogonIPAddress, @tnyUserType=UserType from users.UserInfo with (nolock) where UserID=@intRewardUserID
		--檢查IP是否正確
		IF (dbo.Fn_CheckValidIP(@chvIPAddress) = 0)
		BEGIN
			SET @chvIPAddress = '未知IP'
		END

		--更新獎勵金額
		update Event.Ranking set Reward=@lngRewardMoney where ID=@intRankID

		IF @tnyUserType <> 2 --如果是機器人不送金幣
		BEGIN
			print '送金币'

			--log
			insert into Event.ContestEventLog(UserID,Data) values(@intRewardUserID,'比赛活动=' + @chvContestName + ', 排行=' + convert(varchar,@intRank) + ', 發送獎勵金額 '+ convert(varchar,@lngRewardMoney))
					
			--送金币
			SET @lngAmount = @lngRewardMoney
			EXEC dbo.PrWeb_UpdateUserCash
				@intUserID = @intRewardUserID,
				@sintSourceType = @sintSourceType,
				@lngUpdateAmount = @lngAmount,
				@chvIPAddress = @chvIPAddress,
				@chvMemo = @chvM,
				@lngCurrentAmount = @lngCurrentAmount OUTPUT,
				@chvErrMsg = @chvErrMsg OUTPUT	

			--发送到账邮件
			set @chvMailContent = '亲爱的玩家：
					您的' + @chvContestName + '活动奖励金币:'+dbo.Fn_ShowMoney(@lngAmount)+'已到账
			                                                                                                                                  
				（如有任何疑问请联系客服）'

			--发送系统邮件
			EXEC PrPs_SendSysMail
						@intRewardUserID, 
						@chvT,
						@chvMailContent,
						0,
						NULL
		

		END

	FETCH NEXT FROM cur INTO  @intRankID, @intContestID, @intRewardUserID, @intRank, @tnyType, @lngScore
	END
	CLOSE cur
	DEALLOCATE cur



END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_Event_ContestCalcReward] TO [xbuser]
    AS [dbo];

