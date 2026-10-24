-- =============================================
-- Author:  jason
-- Create date: 2019/04/13
-- Description: 全民代理后台 调整一级下线比例  格式: level1,10,2,1|level2,15,4,2|level3,20,6,3 etc.
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_UserAgent_UserRate_Update]
(
	@intUserID		int,				--用户 ID
	@intSubUserID	int,				--调整用户 ID
	@chvLevelString	varchar(max),		--用户权限
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	if not exists (select top 1 1 from [dbo].[UserAgentInfo] with (nolock) where userid = @intSubUserID and TopUserID = @intUserID)
	begin
		set @chvMsg	= '操作错误 : 无法越级调整比例'
		return -1
	end

	declare @FullstringDelimiter varchar(1) = '|'
	declare @LevelstringDelimiter varchar(1) = ','
	declare @temp_levelstringsplit table (Num int,levelstring varchar(100))

	insert into @temp_levelstringsplit
	select	[id],[Value]
	from [dbo].[fn_Split](@chvLevelString,@FullstringDelimiter)

	
	/*  确认笔数  */
	if ((select count(Num) from @temp_levelstringsplit) <> 6)
	begin
		set @chvMsg	= '操作错误 : 设定比例数据错误'
		return -1
	end

	declare @minno int
	declare @maxno int

	select	@minno = min(Num),
			@maxno = max(Num)
	from @temp_levelstringsplit

	delete from [dbo].[UserAgentUserRate] where userid = @intSubUserID

	while(@minno <= @maxno)
	begin
		declare @string varchar(max) = null

		select	@string = levelstring from @temp_levelstringsplit where Num = @minno

		set @string = replace(@string,',','.')
		
		declare @Userlevel tinyint = null
		declare @Stage1 int = null
		declare @Stage2 int = null
		declare @Stage3 int = null
		
		set	@Userlevel = cast(PARSENAME(@string,4) as int)
		set @Stage1 = cast(PARSENAME(@string,3) as int)
		set @Stage2 = cast(PARSENAME(@string,2) as int)
		set @Stage3 = cast(PARSENAME(@string,1) as int)

		insert into [dbo].[UserAgentUserRate]
			([UserID],[UserLevel],[Stage1],[Stage2],[Stage3])
		values
			(@intSubUserID,@Userlevel,@Stage1,@Stage2,@Stage3)

		set @minno = @minno + 1
	end

	set @chvMsg	= '操作完成'
	return 1
END




