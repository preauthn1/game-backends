-- =============================================
-- Author:  alan
-- Create date: 20180719
-- Description:	建立新平台
--	2019/06/10	Jason		因更新誤蓋檔問題補回若為聯運平台則club新增客服聯絡資訊
--  2019/06/18	CCC			新增代理後台域名預設值
-- =============================================
CREATE procedure [dbo].[PrWeb_CreateNewPlatform]
@intPlatformID int,					--指定平台號
@chvPlatformName varchar(32),		--平台名稱
@intCompany tinyint, --营运平台
@intAreaNum int=10, --預設建立渠道数量
@chvErrMsg	VARCHAR(50) OUTPUT		--返回消息
As
BEGIN

set xact_abort on

--渠道號
DECLARE @intAreaID int
--渠道名稱
DECLARE @chvAreaName varchar(32)
--平台號字串
DECLARE @chvPlatformid varchar(32)
SET @chvPlatformid = CONVERT(varchar,@intPlatformID)


IF @intAreaNum > 200
BEGIN
	SET @chvErrMsg = '错误:預設建立渠道数量不能超过200个'
	RETURN -1
END


IF EXISTS(SELECT 1 FROM [CenterDB].[ServerInfo].[HallVersion] WITH (NOLOCK) WHERE AndroidVersion like '%,'+@chvPlatformid+'_%' OR IOSVersion like '%,'+@chvPlatformid+'_%')
BEGIN
	SET @chvErrMsg = '错误:平台版本号重複'
	RETURN -1
END


IF EXISTS(Select 1 from Area.Platform  WITH (NOLOCK) where PlatformID = @intPlatformID)
BEGIN
	SET @chvErrMsg = '错误:平台编号重複'
	RETURN -1 
END


IF EXISTS(Select 1 from Area.GameList  WITH (NOLOCK) where PlatformID = @intPlatformID)
BEGIN
	SET @chvErrMsg = '错误:平台游戏清单重複'
	RETURN -1 
END

IF EXISTS(Select 1 from Area.PlatformAdminUser  WITH (NOLOCK) where PlatformID = @intPlatformID)
BEGIN
	SET @chvErrMsg = '错误:平台管理者配置重複'
	RETURN -1 
END


IF EXISTS(Select 1 from Area.Advertisement  WITH (NOLOCK) where PlatformID = @intPlatformID)
BEGIN
	SET @chvErrMsg = '错误:平台广告配置重複'
	RETURN -1 
END

IF EXISTS(Select 1 from Area.Area where PlatformID = @intPlatformid)
BEGIN
	PRINT '错误:Area.Area平台號重複'
	RETURN 
END


DECLARE @chvFAQ varchar(5000)
DECLARE @chvUpGradeMsg varchar(1000)
DECLARE @chvAnnMsg varchar(1000)
DECLARE @chvPayTips varchar(1000)
DECLARE @chvAreaConfig varchar(5000)
DECLARE @chvPayAmountConfig varchar(1000)
DECLARE @intDefaultMoney int = 0
DECLARE @intGiftMoney int=50

SET @chvFAQ = 
'<color=#EEC900><size=35>Q:金币说明</size></color>

<color=#FFFFFF><size=30>A:金币是游戏内的流动货币，通过充值获得，充值10元可以获得10金币，金币可以用来体验游戏。</size></color>

<color=#EEC900><size=35>Q:游戏公告</size></color>

<color=#FFFFFF><size=30>A:平台最新动态及游戏更新维护等信息均会以公告的形式告知玩家。</size></color>

<color=#EEC900><size=35>Q:帐号相关</size></color>

<color=#FFFFFF><size=30>A:点击设置，可切换帐号返回到登录界面，输入手机号和密码即可登录。</size></color>

<color=#EEC900><size=35>Q:短信验证</size></color>

<color=#FFFFFF><size=30>A:如果未收到短信验证码请勿频繁验证，稍等五分钟后再进行验证。若始终收不到验证码，可联系客服查询问题。</size></color>

<color=#EEC900><size=35>Q:忘记密码</size></color>

<color=#FFFFFF><size=30>A:忘记密码时可以通过手机号码找回，验证一下即可修改。</size></color>

<color=#EEC900><size=35>Q:人工客服</size></color>

<color=#FFFFFF><size=30>A:游戏内如果遇到任何问题，均可以联系客服寻求帮助，客服小妹会及时帮助您解决游戏中遇到的任何问题。</size></color>'

SET @chvUpGradeMsg = '恭喜你绑定成功,如有问题，请及时联系客服。'
SET @chvAnnMsg = '本游戏是款公平公正的游戏平台，如有问题，请及时联系客服。'
SET @chvPayTips = '1.充值比例为1元=1.00金币。最低充值金额为10元。  2.需要安装“支付宝”或者“微信”客户端。  3.代理为官方认证的安全充值方式。'
SET @chvPayAmountConfig = '1:+100.+300.+500.+1000.+2000|2:+100.+200.+300.+400.+500|3:+100.+200.+300.+400.+500|4:+100.+200.+300.+400.+500'

BEGIN TRAN

	--新增平台
	INSERT INTO [CenterDB].[Area].[Platform]
				([PlatformID],[Name],[UpGradeMsg],[AnnMsg],[FAQContent],[PaySort],[UIFlag],[PayTip],[PayAmountConfig],[PayStatus],[PayCloseTip],
				ShanFuAccountID,ShanFuSign,ShanFuPayAgentUrl,ShanFuPayAgentClientUrl,Company)
	SELECT 
	@intPlatformid as [PlatformID], 
	@chvPlatformName as [Name],
	@chvUpGradeMsg as [UpGradeMsg],
	@chvAnnMsg as [AnnMsg],
	@chvFAQ as [FAQContent],
	'1,2,3,4' as [PaySort],
	'0110' as [UIFlag],
	@chvPayTips as [PayTip],
	@chvPayAmountConfig as [PayAmountConfig],
	1 as [PayStatus],
	'充值关闭' as [PayCloseTip],
	'10020' as ShanFuAccountID,
	'a343a2a5cabca3a6622fbd637d964e9a' as ShanFuSign,
	'http://example.invalid' as ShanFuPayAgentUrl,
	'http://example.invalid/index/login/auto_login' as ShanFuPayAgentClientUrl,
	@intCompany


	--新增渠道
	DECLARE @chvNum varchar(3)
	DECLARE @chvClientType varchar(20)=''
	DECLARE @chvClientName varchar(20)=''


	DECLARE @intNum int = 0
	WHILE @intNum <= @intAreaNum
	BEGIN
		SET @chvNum= RIGHT(REPLICATE('0', 3) + CAST(@intNum as VARCHAR), 3)

		IF @intNum = 0 BEGIN SET @chvClientType='IOS'; SET @chvClientName='苹果' END
		IF @intNum > 0 BEGIN SET @chvClientType='Android'; SET @chvClientName='安卓' END
		 
	 
		SET @chvAreaName = @chvPlatformName + @chvClientName + @chvNum; 
		SET @intAreaID = Convert(int,@chvPlatformid + @chvNum);

		

		IF EXISTS(SELECT 1 FROM [Area].[Area] WHERE AreaID=@intAreaID) 
		BEGIN  
			print @intAreaID
			PRINT '错误:Area.Area渠道號重複'
			ROLLBACK TRAN
			RETURN
		END

		SET @chvAreaConfig = 
		'{"systemConfig":{"Info":{"ShowCaiJinchi":0,"BundleID":"","AppName":"' + @chvPlatformName + 
		'","AppleID":"","ServerOption":0,"PlatformID":' + convert(varchar,@intPlatformID) + 
		',"SiteID":' + convert(varchar,@intAreaId) + 
		',"StaticSiteID":0,"Agent":0,"Template":"0","FlagExchage":0,"ExchangeFontType":0},' + 
		'"JiuLiao":{"JiuLiaoId":"","JiuLiaoSecrety":"","JiuLiaoName":"","DownloadUrl":""},' + 
		'"payConfig":{"UIFlagPay":true,"UIFlagAgent":true,"UIFlagApplyAgent":false,"UIFlagJiuLiao":false,"UIFlagShanFu":false,"UIClub2AgentMode1":false,"UIClub2AgentMode2":false,"UIClub2AgentMode3":true,"UIClub2AgentMode4":false,"UIClub2AgentMode5":false,"UIClub2AgentMode6":false}}}'

		--新增渠道
		Insert into [CenterDB].[Area].[Area] (AreaID,AreaName,ClientPlatform,PlatformID,[Memo],[AdminID],[Sort],FAQContent,PayTips,UpGradeMsg,AnnMsg,AnnMsgShow,SetDefault,AreaConfig,DefaultMoney,GiftMoney,ShopNameID,AgentNameID,ShareURL,ShowConvertLimit,ShowAllGameLimit,ShowVIPAgent,ShowCS,ShowConvertBindPhone,ShowAllGameBindPhone,ShowVIPAgentBindPhone,ShowCSBindPhone,PlaytimeCS,PlaytimeConvert,PlaytimeVIPAgent,CreateTime,AreaStatus) 
		values (@intAreaID ,@chvAreaName, @chvClientType, @intPlatformid, @chvAreaName, 1, 10, @chvFAQ, @chvPayTips, @chvUpGradeMsg, @chvAnnMsg, 0, 0, @chvAreaConfig, @intDefaultMoney,@intGiftMoney,1,1,'',0,0,0,0,0,0,0,0,0,0,0,getdate(),1)
		
		
		--新增渠道的遊戲清單
		insert Area.GameList(platformid,areaid,gameid,sort,[status],IconID)
		select 
		@intPlatformID as PlatformID, @intAreaID as AreaID, G.GameID, 99 as Sort, 1 as Status,
		MIN(A.IconID) as IconID from ServerInfo.GameList G left join area.GameAliasName A on G.GameID=A.GameID
		WHERE G.Status=1
		GROUP BY G.GameID
		
		--新增渠道管理者, 但排除联运帐号(usertype=10)
		insert into area.AreaAdminUser(AdminID,AreaID,PlatformID)
		select u.UserID as AdminID, @intAreaID as AreaID,@intPlatformid as PlatformID from AdminSys.Admin.users u
		where u.UserType <> 10
		
		--新增代理後台域名
		insert into [Club02].[dbo].[DomainSetting] (PlatformID, AreaID, Domain, [Status], CreateTime, PlatformName)
		values (@intPlatformid, @intAreaID, 'https://example.invalid/', 1, getdate(), @chvPlatformName)


		SET @intNum = @intNum + 1
	END
	
	
	
	--新增平台管理者配置, 但排除联运帐号(usertype=10)
	INSERT INTO [CenterDB].[Area].PlatformAdminUser(PlatformID,AdminID)
	SELECT @intPlatformID as PlatformID, UserID from AdminSys.Admin.Users with (nolock) where Usertype <> 10

	--檢查平台版本是否重複
	declare @chvAndroidVersion nvarchar(max), @chvIOSVersion nvarchar(max),@chvAndroidVersion2 nvarchar(max), @chvIOSVersion2 nvarchar(max)

	select 
	@chvAndroidVersion=AndroidVersion,
	@chvAndroidVersion2=AndroidVersion2,
	@chvIOSVersion=IOSVersion,
	@chvIOSVersion2=IOSVersion2
	from serverinfo.HallVersion with (nolock)


	if exists( select 1 from (select value,substring(value, 1,CHARINDEX('_',value)-1) as platformid from dbo.fn_Split (@chvAndroidVersion,',')) as a where platformid=@intPlatformID ) or
	   exists( select 1 from (select value,substring(value, 1,CHARINDEX('_',value)-1) as platformid from dbo.fn_Split (@chvAndroidVersion2,',')) as a where platformid=@intPlatformID ) or
	   exists( select 1 from (select value,substring(value, 1,CHARINDEX('_',value)-1) as platformid from dbo.fn_Split (@chvIOSVersion,',')) as a where platformid=@intPlatformID ) or
	   exists( select 1 from (select value,substring(value, 1,CHARINDEX('_',value)-1) as platformid from dbo.fn_Split (@chvIOSVersion2,',')) as a where platformid=@intPlatformID)
	begin
		PRINT '错误:HallVersion平台版本重複'
		ROLLBACK TRAN
		RETURN
	end

	--修改Hallversion资料, 增加平台版本号
	UPDATE ServerInfo.HallVersion SET 
	AndroidVersion = AndroidVersion + ',' + @chvPlatformid + '_1.0.0', 
	IOSVersion = IOSVersion + ',' + @chvPlatformid + '_1.0.0',
	AndroidVersion2 = AndroidVersion2 + ',' + @chvPlatformid + '_1.0.0', 
	IOSVersion2 = IOSVersion2 + ',' + @chvPlatformid + '_1.0.0'


	--新增渠道包下載資訊
	--Correct : 是否正確 1正確 0不正確
	--IOSTerminate : IOS是否下架 1 已經下架 0上架
	INSERT INTO [Area].[PlatformDownload] (PlatformID, IOS, Android, Official, IOSTerminate, IOSMemo, Correct, Memo)
	VALUES(@intPlatformID,'','','',0,'',0,'')


	--如果是聯運平台,塞入聯運表
	if (@intCompany <> 1) AND not exists(select 1 from  area.UnionPlatform where PlatformID=@intPlatformID)
	begin
		insert into area.UnionPlatform(PlatformID) values(@intPlatformID)
	end
	
	--非聯運平台, 新增全民代理客服清单
	DECLARE @intUnionPlatform INT
	IF EXISTS (SELECT TOP 1 1 FROM Area.UnionPlatform WHERE PlatformID = @intPlatformID)
	BEGIN
		SET @intUnionPlatform = 1
	END
	ELSE
	BEGIN
		SET @intUnionPlatform = 0
	END

	if (@intUnionPlatform = 0)  AND not exists(select 1 from Club.dbo.OfficialUserConfig with (nolock) where PlatformID=@intPlatformID)
	begin
		insert into  Club.dbo.OfficialUserConfig (PlatformID,OfficialUser1,OfficialUser2,OfficialUser3)
		values(@intPlatformID,'481329270','481329270','481329270')
	end

COMMIT TRAN

SET @chvErrMsg = '新增平台成功'
RETURN 1

END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_CreateNewPlatform] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_CreateNewPlatform] TO [xbuser]
    AS [dbo];

