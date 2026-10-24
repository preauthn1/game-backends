-- =============================================
-- Author:  Jason
-- Create date: 2018/07/31
-- Description:	活動列表
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_EventSettingGet]
(
	@chvEventName	varchar(32) = null,
	@dtStatTime		datetime = null,
	@dtEndTime		datetime = null,
	@tnyStatus		int = null
)
AS
BEGIN
	SET NOCOUNT ON

	--set @tnyStatus	= ISNULL(@tnyStatus,1)

	/*  更新用戶資料  */
	------------------------------------------------------------------------------------
	if (@dtStatTime is null) and (@dtEndTime is null)
	begin
		select	[EventID],[EventName],[StartTime],[EndTime],[Status],[GiftCash]
		from [Users].[EventSetting] with (nolock)
		where
			ISNULL(@chvEventName,[EventName]) = [EventName]
			and ISNULL(@tnyStatus,[Status]) = [Status]
			and [Status] >= 0
		order by [EndTime] desc
	end
	else
	begin
		select	[EventID],[EventName],[StartTime],[EndTime],[Status],[GiftCash]
		from [Users].[EventSetting] with (nolock)
		where
			ISNULL(@chvEventName,[EventName]) = [EventName]
			and ISNULL(@tnyStatus,[Status]) = [Status]
			and [Status] >= 0
			and 
				(	@dtStatTime between [StartTime] and [EndTime]
					or 
					@dtEndTime between [StartTime] and [EndTime]
				)
		order by [EndTime] desc
	end
	------------------------------------------------------------------------------------

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_EventSettingGet] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_EventSettingGet] TO [xbuser]
    AS [dbo];

