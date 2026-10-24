-- =============================================
-- Author:  jason
-- Create date: 2019/05/11
-- Description: 列变 系统奖励条件更新
--	2019/05/27		Jason		依照轰达需求从原本2阶调整为6阶
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Split_SystemRate_Update]
(
	@intAdminID		int = null,
	@intPlatformID	int = null,			--平台 ID
	@chvChangeRate	varchar(max) = null,	--  EX: 1,0,10,10|2,0,10,5|3,0,10,1|4,11,20,20|5,11,20,10|6,11,20,2
											--  层级,最低人数,最高人数,奖励金   1~3层级为第一阶  4~6层级为第二阶
											--  7~9层级为第三阶 10~12层级为第四阶 13~15层级为第五阶 16~18层级为第五阶
	@lngChargeConvertAmount bigint = null,	--  平台充值兑换最低达成条件
	@intWithdrawLimit int = null,			--  提领冷却时间  以小时计
	@chvMsg			varchar(250)	OUTPUT
)
AS
BEGIN

	set nocount on

	if (@intAdminID is null) or (@intPlatformID is null)
	begin
		set @chvMsg	= '操作错误 : 用户信息错误'
		select @chvMsg
		return -1
	end

	if not exists (select top 1 1 from [dbo].[AgentPlatformRelations] with (nolock) where adminid = @intAdminID and platformid = @intPlatformID)
	begin
		set @chvMsg	= '操作错误 : 您的权限不足,请通知管理员'
		select @chvMsg
		return -1
	end

	if(@chvChangeRate is not null)
	begin
		declare @FullstringDelimiter varchar(1) = '|'
		declare @LevelstringDelimiter varchar(1) = ','
		declare @temp_levelstringsplit table (Num int,levelstring varchar(100))

		insert into @temp_levelstringsplit
		select	[id],[Value]
		from [dbo].[fn_Split](@chvChangeRate,@FullstringDelimiter)

	
		/*  确认笔数  */
		if ((select count(Num) from @temp_levelstringsplit) <> 15)
		begin
			set @chvMsg	= '操作错误 : 设定比例数据错误'
			return -1
		end

		declare @minno int
		declare @maxno int
		declare @checkmax int = 0
		declare @errorlevel int = 0

		select	@minno = min(Num),
				@maxno = max(Num)
		from @temp_levelstringsplit

		create table #T (PlatformID int,userlevel int,stage1 int,stage2 int,stage3 int)

		while(@minno <= @maxno)
		begin
			declare @string varchar(max) = null

			select	@string = levelstring from @temp_levelstringsplit where Num = @minno

			set @string = replace(@string,',','.')
		
			declare @Userlevel tinyint = null
			declare @MinRecruit int = null
			declare @MaxRecruit int = null
			declare @BonusAmount int = null
			declare @stagelevelstr int = null
		
			set	@Userlevel = cast(PARSENAME(@string,4) as int)
			set @MinRecruit = cast(PARSENAME(@string,3) as int)
			set @MaxRecruit = cast(PARSENAME(@string,2) as int)
			set @BonusAmount = cast(PARSENAME(@string,1) as int)

			set @stagelevelstr = case	when @Userlevel IN(1,2,3) THEN 1
										when @Userlevel IN(4,5,6) THEN 2
										when @Userlevel IN(7,8,9) THEN 3
										when @Userlevel IN(10,11,12) THEN 4
										when @Userlevel IN(13,14,15) THEN 5
										when @Userlevel IN(16,17,18) THEN 6
									end


			update [dbo].[Split_SystemLevelRate]
			set	[MinRecruit1] = case when @Userlevel in (1,4,7,10,13,16) then @MinRecruit else [MinRecruit1] end,
				[MaxRecruit1] = case when @Userlevel in (1,4,7,10,13,16) then @MaxRecruit else [MaxRecruit1] end,
				[Stage1] = case when @Userlevel in (1,4,7,10,13,16) then @BonusAmount else [Stage1] end,
				[MinRecruit2] = case when @Userlevel in (2,5,8,11,14,17) then @MinRecruit else [MinRecruit2] end,
				[MaxRecruit2] = case when @Userlevel in (2,5,8,11,14,17) then @MaxRecruit else [MaxRecruit2] end,
				[Stage2] = case when @Userlevel in (2,5,8,11,14,17) then @BonusAmount else [Stage2] end,
				[MinRecruit3] = case when @Userlevel in (3,6,9,12,15,18) then @MinRecruit else [MinRecruit3] end,
				[MaxRecruit3] = case when @Userlevel in (3,6,9,12,15,18) then @MaxRecruit else [MaxRecruit3] end,
				[Stage3] = case when @Userlevel in (3,6,9,12,15,18) then @BonusAmount else [Stage3] end
			where
				level = @stagelevelstr
				and platformid = @intPlatformID

			if @@rowcount <> 1
			begin
				set @chvMsg	= '操作失败 : 设定第' + cast(@stagelevelstr as varchar) + '阶条件失败'
				return -1
			end

			set @minno = @minno + 1
		end
	end


	if(@lngChargeConvertAmount is not null)
	begin
		update [dbo].[Split_SystemLevelRate]
		set [TaxAmount] = @lngChargeConvertAmount
		where
			platformid = @intPlatformID

		if @@rowcount <> 5
		begin
			set @chvMsg	= '操作失败 : 设定充值兑换最低条件失败'
			return -1
		end
	end

	if(@intWithdrawLimit is not null)
	begin
		update [dbo].[Split_SystemLevelRate]
		set [WithdrawLimit] = @intWithdrawLimit
		where
			platformid = @intPlatformID

		if @@rowcount <> 5
		begin
			set @chvMsg	= '操作失败 : 设定提领冷却时间失败'
			return -1
		end
	end


	set @chvMsg	= '操作完成'
	return 1
END










