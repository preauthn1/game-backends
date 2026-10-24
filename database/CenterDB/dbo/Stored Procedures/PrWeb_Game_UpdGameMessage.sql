-- =============================================
-- Author:  alan
-- Create date: 2019/04/02
-- Description:	修改跑馬燈公告
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Game_UpdGameMessage]
@intAdminID		int,
@intId			int,
@intPlatformID	int=null,
@chvMessageContent varchar(512)=null,
@dtBeginTime	datetime=null,
@dtEndTime		datetime=null,
@intInverval	int=null,
@tnyStatus		tinyint=null,
@tnyAllPlatform		tinyint=null, --是否為全平台公告 0否 1是
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

	update	Game.GameMessage
	set PlatformID=isnull(@intPlatformID,PlatformID),
	MsgContent=isnull(@chvMessageContent,MsgContent),
	BeginTime=isnull(@dtBeginTime,BeginTime),
	EndTime=isnull(@dtEndTime,EndTime),
	Interval=isnull(@intInverval,Interval),
	Status=isnull(@tnyStatus,Status),
	AllPlatform=isnull(@tnyAllPlatform,AllPlatform)
	where Id = @intId

	set @chvErrMsg = ''
	return 1
END




GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Game_UpdGameMessage] TO [GameUser]
    AS [dbo];

