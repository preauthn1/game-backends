
-- =============================================
-- Author:  jason
-- Create date: 20190604
-- Description: 裂变 - 用户列表 取下層用戶資料
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_GetUserInfo_LevelUsers]
(
	@intUserID	int,
	@intLevel	INT,			-- 1:B level 2:C level 3:D level
	@chvMsg		varchar(250)	OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON
	

	declare @topuserid int
	declare @userlevel int
	declare @useridstr varchar(20)


	select	@userlevel = [StageLevel],
			@topuserid = [TopUserID]
	from [dbo].[Split_UserInfo] with (nolock)
	where
		[UserID] = @intUserID
	
	set @useridstr = cast(@intUserID as varchar)
	set @topuserid = isnull(@topuserid,@intUserID)


	/*  取得管理者可读取平台  */
	declare @temp_userlist table(userid int,createtime datetime)

	if(@topuserid is not null)
	begin
		insert into @temp_userlist
		select	a.userid,
				b.createtime
		from (
				select	userid
				from (
						select	a.userid, a.relations
						from [dbo].[Split_UserInfo] as a with (nolock)
						join [dbo].[Split_UserMission] as b with (nolock) on a.UserID = b.UserID
						where
							a.[TopUserID] = @topuserid
							and a.[StageLevel] = (@userlevel + @intLevel)
							and (b.MobileBind=1 and (b.AlipayBind=1 or b.UnionPayBind=1) and (b.ChargeBind=1 or b.ConvertBind=1))
					) as a
				where
					relations like '%' + @useridstr + '%'
			) as a
			inner join centerdb.users.userinfo as b with (nolock) on a.userid = b.userid
	end

	
	select	userid,createtime
	from @temp_userlist

	return 1
END


