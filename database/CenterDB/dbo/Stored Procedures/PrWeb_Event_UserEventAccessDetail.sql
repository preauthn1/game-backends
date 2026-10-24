
-- =============================================
-- Author:  Jason
-- Create date: 2018/07/31
-- Description:	活動資格明細查詢
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_UserEventAccessDetail]
(
	@sintEventID	smallint = null,
	@intUserID		int = null,
	@dtStatTime		datetime = null,
	@dtEndTime		datetime = null,
	@intPageIndex		int=1,
	@intPageSize		int=20,
	@intPageCount		int output
)
AS
BEGIN
	SET NOCOUNT ON

	set @intPageCount = 0
	
	/*  回傳資料  */
	------------------------------------------------------------------------------------
	SELECT @intPageCount = COUNT(1) 
	from [Users].[EventDetailLog] as a with (nolock)
		inner join [Users].[EventSetting] as b with (nolock)
			on a.[EventID] = b.EventID
		inner join [Users].[UserInfo] as c with (nolock)
			on a.userid = c.userid
	where
		isnull(@sintEventID,a.[EventID]) = a.[EventID]
		and isnull(@intUserID,a.[UserID]) = a.[UserID]
		and isnull(@dtStatTime,a.[CreateTime]) >= a.[CreateTime]
		and isnull(@dtEndTime,a.[CreateTime]) <= a.[CreateTime]
	
	;with tab as 
	(
		select	row_number() over (order by a.id desc) as rowid,
				a.[ID],
				a.[EventID],
				b.EventName,
				b.StartTime,
				b.EndTime,
				a.[UserID],
				a.LoginName,
				a.[CreateTime],
				a.[GiftCash],
				a.[IPAddress]
		from [Users].[EventDetailLog] as a with (nolock)
			inner join [Users].[EventSetting] as b with (nolock)
				on a.[EventID] = b.EventID
		where
			isnull(@sintEventID,a.[EventID]) = a.[EventID]
			and isnull(@intUserID,a.[UserID]) = a.[UserID]
			and isnull(@dtStatTime,a.[CreateTime]) >= a.[CreateTime]
			and isnull(@dtEndTime,a.[CreateTime]) <= a.[CreateTime]
	)
	SELECT * FROM tab WHERE rowid between @intPageSize * (@intPageIndex -1) + 1 and @intPageSize * @intPageIndex	
	------------------------------------------------------------------------------------

	set @intPageCount = isnull(@intPageCount,0)
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_UserEventAccessDetail] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_UserEventAccessDetail] TO [xbuser]
    AS [dbo];

