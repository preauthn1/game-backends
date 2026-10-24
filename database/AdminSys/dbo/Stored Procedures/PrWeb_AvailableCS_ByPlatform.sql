-- =============================================
-- Author:		alan
-- Create date: 2019/3/27
-- Description:	依據平台指派客服人員
-- changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AvailableCS_ByPlatform]
@intPlatformID int
AS
BEGIN

	SET NOCOUNT ON;
	
	--找出這個平台的所有後台使用者
	select AdminId into #allAdmin from CenterDB.Area.PlatformAdminUser with (nolock) where PlatformID=@intPlatformID

	select top 1 UserID, LoginName from [Admin].[Users] with (nolock) 
	where UserType = 9 --客服身分
	and lastlogontime > dateadd (HOUR,-2,getdate()) 
	and UserID in (select AdminId from #allAdmin)
	order by NEWID()
	
	if @@ROWCOUNT = 0 --如果沒有找到任何人, 直接選定一個客服
		select top 1 UserID, LoginName from [Admin].[Users] with (nolock) where UserType = 9
		and UserID in (select AdminId from #allAdmin)

END
