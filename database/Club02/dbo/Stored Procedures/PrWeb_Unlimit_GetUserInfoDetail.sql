
-- =============================================
-- Author:  jason
-- Create date: 20190604
-- Description: 后台  无极限-用户信息 階層明細
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Unlimit_GetUserInfoDetail]
(
	@intUserID int,
	@chvMsg varchar(100) OUTPUT
)
AS
BEGIN

	SET NOCOUNT ON


	declare @userlevelstr varchar(max)
	declare @userlevel	int
	declare @starttime datetime = dateadd(day,0,datediff(day,0,getdate()))
	declare @endtime datetime = dateadd(day,1,datediff(day,0,getdate()))


	if(@intUserID is null)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end

	if not exists (select top 1 1 from [dbo].[Unlimit_UserInfo] with (nolock) where userid = @intUserID)
	begin
		set @chvMsg = '操作错误 : 用户不存在'
		return -1
	end


	/*  取查詢用戶基本資料  */
	select	@userlevelstr = relations,
			@userlevel = [StageLevel]
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where userid = @intUserID 

	declare @Temp_userlist table (Userid int,usertype int,Topuserid int,Memo nvarchar(100))

	insert into @temp_userlist
	select	userid,
			usertype = case when stagelevel = @userlevel + 1 then 0 else 1 end,
			cast(case when relations is not null and stagelevel > 1 then substring(relations,1,charindex(',',relations)-1) else 0 end as int),
			Memo
	from [dbo].[Unlimit_UserInfo] with (nolock)
	where
		userid <> @intUserID
		and [Relations] like '' + @userlevelstr + '%'

	/*  取资料 */
	select	distinct
			a.Userid,
			[StorageAmount] = isnull(c.[Amount],0) + isnull(d.[Amount],0),
			[SumCharge] = isnull(b.[SumCharge],0),
			[SumConvert] = isnull(b.[SumConvert],0),
			[TodayCharge] = isnull(e.[ChargeAmount],0) * 100,
			[TodayConvert] = abs(isnull(e.[ConvertAmount],0)),
			Topuserid = isnull(a.Topuserid,0),
			Memo = isnull(a.Memo,'')
	from @temp_userlist as a
		left join (
					select	userid,
							[SumCharge] = isnull([SumCharge],0) + isnull([SumAgentCharge],0),
							[SumConvert] = isnull([SumConvert],0)
					from centerdb.users.userinfo with (nolock)
					where
						userid in (select userid from @temp_userlist)
				) as b on a.userid = b.userid
		left join (
					select	userid,[Amount]
					from centerdb.[Money].[Cash] with (nolock)
					where
						userid in (select userid from @temp_userlist)
				) as c on a.userid = c.userid
		left join (
					select	userid,[Amount]
					from centerdb.[Money].[Bank] with (nolock)
					where
						userid in (select userid from @temp_userlist)
				) as d on a.userid = d.userid
		left join (
					select	userid,
							[ChargeAmount] = sum(isnull([ChargeAmount],0)),
							[ConvertAmount] = sum(isnull([ConvertAmount],0))
					from logdb.[Stat].[StatisticsUserAmount_Day] with (nolock)
					where
						userid in (select userid from @temp_userlist)
						and [LogDate] = @starttime
					group by userid
				) as e on a.userid = e.userid


	set @chvMsg = '操作成功'
	return 1
END




