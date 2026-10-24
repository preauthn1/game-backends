-- =============================================
-- Author:  alan
-- Create date: 2019/04/02
-- Description:	新增跑馬燈公告
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_AddGameMessage]
@intAdminID		int,
@intPlatformID	int,
@chvMessageContent varchar(512),
@dtBeginTime	datetime,
@dtEndTime		datetime=null,
@intInverval	int,
@tnyStatus		tinyint,
@tnyAllPlatform		tinyint=0, --是否為全平台公告 0否 1是
@chvErrMsg varchar(128) OUTPUT
AS
BEGIN

	--渠道篩選
	DECLARE @Temp_PlatformID TABLE (PlatformID	INT)
	INSERT INTO @Temp_PlatformID Select PlatformID From CenterDB.Area.PlatformAdminUser where AdminID=@intAdminID


	if not exists(select 1 from @Temp_PlatformID where PlatformID=@intPlatformID)
	begin
		set @chvErrMsg = '权限不足, 无法新增'
		return 0
	end

	if @dtEndTime is null 
		set @dtEndTime = '2050-1-1'
	
	Insert into Game.GameMessage(PlatformID,MsgContent,BeginTime,EndTime,Interval,Status,Allplatform)
	values(@intPlatformID,@chvMessageContent,@dtBeginTime,@dtEndTime,@intInverval,@tnyStatus, @tnyAllPlatform)

	set @chvErrMsg=''
	return 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_AddGameMessage] TO [GameUser]
    AS [dbo];

