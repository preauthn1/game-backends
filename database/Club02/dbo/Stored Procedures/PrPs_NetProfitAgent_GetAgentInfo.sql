-- =============================================
-- Author:  jason
-- Create date: 2019/04/15
-- Description: USER 玩家/代理信息(净利润)
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_NetProfitAgent_GetAgentInfo]
(
	@intUserID		int				--用户 ID
)
AS
BEGIN

	set nocount on

	declare @regtime datetime
	declare @charge bigint
	declare @withdraw bigint
	declare @status tinyint
	declare @subuser int
	declare @subagent int
	declare @subuseragentamount bigint
	declare @chvQRUrl varchar(250)	
	declare @qrcode varchar(8)
	declare @UpperUserID int

	if not exists (select top 1 1 from [dbo].[NetProfitAgent_UserInfo] with (nolock) where userid = @intUserID)
	begin
		declare @temp_table	table (userid int,regdate datetime,ChargeAmount bigint,SelfWithDraw bigint,[Status] varchar(2),
		                          [TotalUsers] int,[SumSubUsers] int,[SumSubAgent] int,[SumSubWithDrawAmount] bigint,
								  QRCode varchar(250),[InviteCode] varchar(8))
		select	UserID,
				RegDate,
				ChargeAmount,
				SelfWithDraw,
				[Status],
				[TotalUsers],
				[SumSubUsers],
				[SumSubAgent],
				[SumSubWithDrawAmount],
				[QRCode],
				[InviteCode]
		from @temp_table
		return 1
	end


	/*  取自己取下线资料  */
	select	@regtime = [CreateTime],
			@status = [Status],
			@subuser = [SumSubUser],
			@subagent = [SumSubAgent],
			@charge = [SumChargeAmount],
			@withdraw = [SumWithDrawAmount],
			@UpperUserID = [TopUserID]
	from [dbo].[NetProfitAgent_UserInfo] with (nolock)
	where
		userid = @intUserID
	
	
	select	@charge = @charge + [SumSelfChargeAmount],
			@withdraw = @withdraw + [SumWithDrawAmount],
			@subuseragentamount = [SumWithDrawAmount]
	from (
			select	[SumSelfChargeAmount] = sum(isnull([SumChargeAmount],0)),
					[SumWithDrawAmount] = sum(isnull([SumWithDrawAmount],0))
			from [dbo].[NetProfitAgent_UserInfo] with (nolock)
			where
				topuserid = @intUserID
		) as a

	set @regtime = isnull(@regtime,'1970-01-01')
	set @charge = isnull(@charge,0)
	set @withdraw = isnull(@withdraw,0)
	set @subuser = isnull(@subuser,0)
	set @subagent = isnull(@subagent,0)
	set @subuseragentamount = isnull(@subuseragentamount,0)
		

	select	top 1 @chvQRUrl = [Domain] + '/?a=' + QRCode,
			@qrcode = QRCode
	from [dbo].[UserAgentQRCode] as a with (nolock)
		inner join [dbo].[DomainSetting] as b with (nolock) on a.domainid = b.id
	where
		userid = @intUserID

	set @chvQRUrl = isnull(@chvQRUrl,'')
	set @qrcode = isnull(@qrcode,'')
	set @UpperUserID = isnull(@UpperUserID,0)

	/*  回传资料  */
	select	UserID = @intUserID,
			RegDate = @regtime,
			ChargeAmount = @charge,
			SelfWithDraw = @withdraw,
			[Status] = case when @status < 2 then '否' else '是' end,
			[TotalUsers] =  @subuser + @subagent,
			[SumSubUsers] = @subuser,
			[SumSubAgent] = @subagent,
			[SumSubWithDrawAmount] = @subuseragentamount,
			[QRCode] = @chvQRUrl,
			[InviteCode] = @qrcode,
			UpperUserID = @UpperUserID
	return 1
END





