
-- =============================================
-- Author:  Jason
-- Create date: 2018/07/31
-- Description:	活動資格查詢
--	20180803	alan	調整撈取方式,日期排序
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Event_UserEventAccessGet]
(
	@sintEventID	smallint = null,
	@intUserID		int = null,
	@chvLoginName	varchar(32) = null,
	@dtStatTime		datetime = null,
	@dtEndTime		datetime = null
)
AS
BEGIN
	SET NOCOUNT ON

	--如果沒有時間區間,限制最多只撈出1000筆
	IF @dtStatTime IS NULL AND @dtEndTime IS NULL
	BEGIN

		select top 1000	a.[EventID],b.[EventName],a.UserID,a.LoginName,a.[GrantTimes],a.UpdateTime			
		from [Users].[EventLog] as a with (nolock)
			inner join [Users].[EventSetting] as b with (nolock)
				on a.EventID = b.EventID
		where
			ISNULL(@sintEventID,a.[EventID]) = a.[EventID]
			and ISNULL(@intUserID,a.[UserID]) = a.[UserID]
			and ISNULL(@chvLoginName,a.LoginName) = a.LoginName
			order by  a.UpdateTime desc,a.LoginName desc

			return
	END



	select	a.[EventID],b.[EventName],a.UserID,a.LoginName,a.[GrantTimes],a.UpdateTime			
	from [Users].[EventLog] as a with (nolock)
		inner join [Users].[EventSetting] as b with (nolock)
			on a.EventID = b.EventID
	where
		ISNULL(@sintEventID,a.[EventID]) = a.[EventID]
		and ISNULL(@intUserID,a.[UserID]) = a.[UserID]
		and ISNULL(@chvLoginName,a.LoginName) = a.LoginName
		and ISNULL(@dtStatTime,a.[UpdateTime]) <= a.[UpdateTime]
		and ISNULL(@dtEndTime,a.[UpdateTime]) >= a.[UpdateTime]
		order by  a.UpdateTime desc,a.LoginName desc


END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_UserEventAccessGet] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Event_UserEventAccessGet] TO [xbuser]
    AS [dbo];

