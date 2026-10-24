-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<Description,,>
-- Changelog:
--		2018/3/13 alan 修改
--		add @chvUnionpayAccount parameter
--	2018/05/16	Jason		#263 用戶統計-註冊用戶查詢增加查詢條件 新增@intAreaID參數
--	2018/05/18	Jason		#271 用戶統計-註冊用戶查詢增加現金資訊與統計註冊人數,充值人數 增加money.amount
--	2018/05/19	harry		#289 用戶統計-註冊用戶查詢增加 Area.AreaName
--	2018/05/30	Jason		#378 排序調整
--  2018/06/05  Emily       #411 用戶統計頁面 註冊用戶查詢可以用創建時間排序
--	2018/11/10	alan		增加等級搜尋
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_GetUserInfo]
@intUserID						int=null,--用户编号
@chvLoginName					varchar(32)=null,--用户名 
@dtmRegBeginTime				DATETIME=null,--注册开始时间
@dtmRegEndTime					datetime=null,--注册结束时间
@dtmLastlogonBeginTime			datetime=null,--最后登录开始时间
@dtmLastlogonEndTime			datetime=null,--最后登录结束时间
@chvRegIpAddress				varchar(32)=null,--注册IP地址
@intUserType					int=null,--用户类型
@intHdType						int=null,--硬件类型
@chvHdCode						varchar(512)=null,--硬件编号
@chvLastlogonIPAddress			varchar(512)=null,--最后登录IP地址
@intSex							int=null,--性别
@intGrandID						int=null,--等級
@dmlConvertChargeRateBegin		decimal(18,2)=null,--充值兑换比例开始范围
@dmlConvertChargeRateEnd		decimal(18,2)=null,--充值兑换比例结束范围
@lngChargeAmountBegin			bigint=null,		--充值数量开始范围
@lngChargeAmountEnd				bigint=null,		--充值数量开始范围
@lngConvertAmountBegin			bigint=null,		--兑换数量开始范围
@lngConvertAmountEnd			bigint=null,		--兑换数量开始范围
@chvUnionpayAccount				varchar(64)=null,	--銀行卡账号
@chvAlipayAccount				varchar(64)=null,	--支付宝账号
@chvRealName					varchar(64)=null,	--真实姓名
@tnyColumns						TINYINT = 0,		--0:總充值 1:總兌換
@tnySort						TINYINT = 0,		--0:降序 1:升序
@intPageIndex					int=1,
@intPageSize					int=20,
@intPageCount					int output,
@intAreaID						INT = NULL
AS
BEGIN



		IF 
		@intUserID is null and
		@chvLoginName is null and
		@dtmRegBeginTime is null and			
		@dtmRegEndTime is null and			
		@dtmLastlogonBeginTime is null and	
		@dtmLastlogonEndTime is null and
		@chvRegIpAddress is null and	
		@intUserType is null and
		@intHdType is null and
		@chvHdCode is null and
		@chvLastlogonIPAddress is null and
		@intSex is null and
		@intGrandID is null and
		@dmlConvertChargeRateBegin is null and
		@dmlConvertChargeRateEnd is null and
		@lngChargeAmountBegin is null and
		@lngChargeAmountEnd is null and
		@lngConvertAmountBegin is null and
		@lngConvertAmountEnd is null and
		@chvUnionpayAccount is null and
		@chvAlipayAccount is null and
		@chvRealName is null 
		BEGIN
			SET @dtmRegBeginTime = DateADD(minute,-30,getdate())
			SET @dtmRegEndTime = getdate()
		END
		
		
		SELECT @intPageCount=COUNT(a.UserID)
		from Users.UserInfo as a with (nolock)
		where (@intUserID is null or a.UserID=@intUserID)
			AND ISNULL(@intAreaID,a.AreaID) = a.AreaID
			and (@chvLoginName is null or a.LoginName=@chvLoginName)
			and (@dtmRegBeginTime is null or a.CreateTime>=@dtmRegBeginTime)
			and (@dtmRegEndTime is null or a.CreateTime<=@dtmRegEndTime)
			and (@dtmLastlogonBeginTime is null or a.LastLogonTime>=@dtmLastlogonBeginTime)
			and (@dtmLastlogonEndTime is null or a.LastLogonTime<=@dtmLastlogonEndTime)
			and (@chvRegIpAddress is null or a.CreateIPAddress=@chvRegIpAddress)
			and (@intUserType is null or a.UserType=@intUserType)
			and (@intHdType is null or a.HDType=@intHdType)
			and (@chvHdCode is  null or a.HDCode=@chvHdCode)
			and (@intGrandID is null or a.GradeID=@intGrandID)
			and (@lngChargeAmountBegin is null OR a.SumCharge >= @lngChargeAmountBegin)
			and (@lngChargeAmountEnd is null OR a.SumCharge <= @lngChargeAmountEnd)
			and (@lngConvertAmountBegin is null OR a.SumConvert >= @lngConvertAmountBegin)
			and (@lngConvertAmountEnd is null OR a.SumConvert <= @lngConvertAmountEnd)
			AND (@dmlConvertChargeRateBegin IS NULL OR 1.0*a.SumConvert/case when a.SumCharge=0 then 1 else a.SumCharge end >= @dmlConvertChargeRateBegin)
			AND (@dmlConvertChargeRateEnd IS NULL OR 1.0*a.SumConvert/case when a.SumCharge=0 then 1 else a.SumCharge end <= @dmlConvertChargeRateEnd)
			AND (@chvAlipayAccount IS NULL OR a.AlipayAccount like '%'+@chvAlipayAccount+'%')
			AND (@chvUnionpayAccount IS NULL OR a.UnionpayAccount like '%'+@chvUnionpayAccount+'%')
			AND (@chvRealName IS NULL OR a.RealName like '%'+@chvRealName+'%')
			
		;with tb as (
			SELECT a.UserID as 'Id', a.*, cashamount = isnull(f.amount,0), e.Name as 'Area.Platform.Name', d.AreaName as 'Area.AreaName', ISNULL(b.UserID,0) as block,
			--ROW_NUMBER() over(order by a.CreateTime desc) as rowid
			ROW_NUMBER() OVER (ORDER BY CASE	WHEN  @tnyColumns = 0 AND @tnySort = 0 THEN a.sumcharge
												WHEN  @tnyColumns = 0 AND @tnySort = 1 THEN (0 - a.sumcharge)
												WHEN  @tnyColumns = 1 AND @tnySort = 0 THEN a.sumconvert
												WHEN  @tnyColumns = 1 AND @tnySort = 1 THEN (0 - a.sumconvert)
												WHEN  @tnyColumns = 2 AND  @tnySort = 0 THEN DATEDIFF(SECOND,'1970-01-01', a.CreateTime)
											    WHEN  @tnyColumns = 2 AND  @tnySort = 1 THEN 0 - DATEDIFF(SECOND,'1970-01-01', a.CreateTime)
											END DESC) AS rowid
		from Users.UserInfo as a with (nolock)
		INNER JOIN Area.Area as d  with (nolock) on a.AreaID = d.AreaID
		INNER JOIN Area.Platform as e with (nolock) on d.PlatformID = e.PlatformID
		left join [Money].[Cash] as f with (nolock) on a.userid = f.userid
		left join [Game].[ForbidUser] as b with (nolock) on a.UserID=b.UserID
		where (@intUserID is null or a.UserID=@intUserID)
			AND ISNULL(@intAreaID,a.AreaID) = a.AreaID
			and (@chvLoginName is null or a.LoginName=@chvLoginName)
			and (@dtmRegBeginTime is null or a.CreateTime>=@dtmRegBeginTime)
			and (@dtmRegEndTime is null or a.CreateTime<=@dtmRegEndTime)
			and (@dtmLastlogonBeginTime is null or a.LastLogonTime>=@dtmLastlogonBeginTime)
			and (@dtmLastlogonEndTime is null or a.LastLogonTime<=@dtmLastlogonEndTime)
			and (@chvRegIpAddress is null or a.CreateIPAddress=@chvRegIpAddress)
			and (@intUserType is null or a.UserType=@intUserType)
			and (@intHdType is null or a.HDType=@intHdType)
			and (@chvHdCode is  null or a.HDCode=@chvHdCode)
			and (@intGrandID is null or a.GradeID=@intGrandID)
			and (@lngChargeAmountBegin is null OR a.SumCharge >= @lngChargeAmountBegin)
			and (@lngChargeAmountEnd is null OR a.SumCharge <= @lngChargeAmountEnd)
			and (@lngConvertAmountBegin is null OR a.SumConvert >= @lngConvertAmountBegin)
			and (@lngConvertAmountEnd is null OR a.SumConvert <= @lngConvertAmountEnd)
			AND (@dmlConvertChargeRateBegin IS NULL OR 1.0*a.SumConvert/case when a.SumCharge=0 then 1 else a.SumCharge end >= @dmlConvertChargeRateBegin)
			AND (@dmlConvertChargeRateEnd IS NULL OR 1.0*a.SumConvert/case when a.SumCharge=0 then 1 else a.SumCharge end <= @dmlConvertChargeRateEnd)
			AND (@chvAlipayAccount IS NULL OR a.AlipayAccount like '%'+@chvAlipayAccount+'%')
			AND (@chvUnionpayAccount IS NULL OR a.UnionpayAccount like '%'+@chvUnionpayAccount+'%')
			AND (@chvRealName IS NULL OR a.RealName like '%'+@chvRealName+'%')
		)									
		select * from tb where rowid between @intPageSize * (@intPageIndex-1) +1  and @intPageSize * @intPageIndex
		
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserInfo] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_User_GetUserInfo] TO [xbuser]
    AS [dbo];

