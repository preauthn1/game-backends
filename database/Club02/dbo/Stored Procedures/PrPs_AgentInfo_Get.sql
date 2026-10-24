



-- =============================================
-- Author:  emily
-- Create date: 2018/08/09
-- Description:	取得会员相關資訊
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_AgentInfo_Get]
	@intUserID			INT,	                    --用户ID	
	@chvErrMsg			VARCHAR(128) OUTPUT			--返回消息	
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @intPlatformID int=0
	DECLARE @isAgent bit = 0
	DECLARE @ID uniqueidentifier
	DECLARE @Class tinyint
	DECLARE @Level tinyint
	DECLARE @ParentID uniqueidentifier	--上级代理id
	DECLARE @ParentClass tinyint		--上级代理等级
	DECLARE @ParentUserID INT			--上级代理userid
	DECLARE @ParentWeixin VARCHAR(50)	--上级代理微信号
	DECLARE @NextGradeName NVARCHAR(50) --上级设定的下个阶级名称
	DECLARE @NextDistributeRate INT		--上级设定的下个阶级收益比例
	DECLARE @NextUpgradeLimit BIGINT		--上级设定的下个阶级升级流水条件
	DECLARE @YesterdayFlows BIGINT	--昨日流水
	DECLARE @CurrentFlows BIGINT	--今日流水
	DECLARE @YesterdayIncome BIGINT	--昨日收益
	DECLARE @CurrentIncome BIGINT	--今日收益
	DECLARE @Yesterday INT = Convert(INT,Convert(varchar,Getdate()-1,112))
	DECLARE @Today INT =  Convert(INT,Convert(varchar,Getdate(),112))

	DECLARE @chvOfficialUser1 varchar(50)=''
	DECLARE @chvOfficialUser2 varchar(50)=''
	DECLARE @chvOfficialUser3 varchar(50)=''
	DECLARE @intUser1Type tinyint=1
	DECLARE @intUser2Type tinyint=1
	DECLARE @intUser3Type tinyint=1
	SET @chvErrMsg = ''


	--取得平台號
	select @intPlatformID=PlatformID from CenterDB.Area.Area with (nolock) where areaid = (select areaid from CenterDB.Users.UserInfo with (nolock) where userid = @intUserID)
		
	--取得總代理的客服號
	SELECT  @chvOfficialUser1=OfficialUser1,  @chvOfficialUser2=OfficialUser2,  @chvOfficialUser3=OfficialUser3,
	@intUser1Type=User1Type, @intUser2Type=User2Type, @intUser3Type=User3Type
	From dbo.OfficialUserConfig with (nolock) where platformID = @intPlatformID


	--檢查是否是club會員
	IF NOT EXISTS(SELECT 1 FROM [dbo].[AgentUsers] WITH (NOLOCK) WHERE UserID = @intUserID)
	BEGIN
		--SET @chvErrMsg = '非推广系统会员'
		--RETURN -1

		SELECT
			   0 as ID
			  ,0 as UserID
			  ,0 as Class
			  ,0 as [Status]
			  ,0 as [Level]
			  ,'' as InviteCode
			  ,0 as ParentUserID
			  ,0 as ParentClass
			  ,'' as ParentWeixin
			  ,0 as TotalFlows
			  ,0 as TotalIncome
			  ,0 as RemainAmount
			  ,0 as YesterdayFlows
			  ,0 as CurrentFlows
			  ,0 as YesterdayIncome
			  ,0 as CurrentIncome
			  ,@intUser1Type as User1Type
			  ,@intUser2Type as User2Type
			  ,@intUser3Type as User3Type
			  ,ISNULL(@chvOfficialUser1,'') as OfficialUser1
			  ,ISNULL(@chvOfficialUser2,'') as OfficialUser2
			  ,ISNULL(@chvOfficialUser3,'') as OfficialUser3 
			  ,'' as DownloadURL
			  ,'' as QRUrl
			  RETURN 1
	END

	--取得用户基本資料
	SELECT @ID = ID, @Level = [Level], @ParentID = ParentID, @Class=Class FROM [dbo].[AgentUsers] WITH(NOLOCK) WHERE UserID = @intUserID 
	--取得我的上級代理ID
	SELECT @ParentUserID = [UserID], @ParentClass=Class, @ParentWeixin=Weixin FROM [dbo].[AgentUsers] WITH(NOLOCK) WHERE ID = @ParentID


	--是否是一般会员身分
	IF @Class = 99 -- or @Class = 0 
	BEGIN

		SELECT
			   U.ID
			  ,U.UserID
			  ,U.Class
			  ,U.[Status]
			  ,U.[Level]
			  ,ISNULL(U.InviteCode,'') as InviteCode
			  ,ISNULL(@ParentUserID,0) as ParentUserID
			  ,ISNULL(@ParentClass,0) as ParentClass
			  ,ISNULL(@ParentWeixin,'') as ParentWeixin
			  ,U.TotalFlows
			  ,0 as TotalIncome
			  ,0 as RemainAmount
			  ,0 as YesterdayFlows
			  ,0 as CurrentFlows
			  ,0 as YesterdayIncome
			  ,0 as CurrentIncome
			  ,@intUser1Type as User1Type
			  ,@intUser2Type as User2Type
			  ,@intUser3Type as User3Type
			  ,ISNULL(@chvOfficialUser1,'') as OfficialUser1
			  ,ISNULL(@chvOfficialUser2,'') as OfficialUser2
			  ,ISNULL(@chvOfficialUser3,'') as OfficialUser3
			  ,'' as DownloadURL
			  ,'' as QRUrl
		FROM [dbo].[AgentUsers] AS U  
		WHERE  U.ID = @ID

		--昨日流水,今日流水
		--SELECT @intUserID as userID,0 as Yesterday,0 as Today
		
		RETURN 1
	END



	--取得昨天與今天流水/收益
	SELECT @YesterdayFlows = [Flows] FROM [dbo].[AgentFlowsSummary] WITH(NOLOCK) WHERE ID = @ID AND YYYYMMDD = @Yesterday  
	SELECT @CurrentFlows = [Flows] FROM [dbo].[AgentFlowsSummary] WITH(NOLOCK) WHERE ID = @ID AND YYYYMMDD = @Today 
	SELECT @YesterdayIncome=Sum(Income) FROM [dbo].[AgentProfitLog] WITH(NOLOCK) WHERE ID = @ID AND YYYYMMDD = @Yesterday  
	SELECT @CurrentIncome=Sum(Income) FROM [dbo].[AgentProfitLog] WITH(NOLOCK) WHERE ID = @ID AND YYYYMMDD = @Today 
	
	
	--撈取代理資料
	SELECT
	    U.ID
		,U.UserID
	    ,U.Class
		,U.[Status]
		,U.[Level]
		,ISNULL(U.InviteCode,'') as InviteCode
		,ISNULL(@ParentUserID,0) as ParentUserID
		,ISNULL(@ParentClass,0) as ParentClass
		,ISNULL(@ParentWeixin,'') as ParentWeixin
		,U.TotalFlows
		,U.TotalIncome
		,U.RemainAmount
		,YesterdayFlows = ISNULL(@YesterdayFlows,0)
		,CurrentFlows = ISNULL(@CurrentFlows,0)
		,YesterdayIncome = ISNULL(@YesterdayIncome,0)
		,CurrentIncome = ISNULL(@CurrentIncome,0)
		,@intUser1Type as User1Type
		,@intUser2Type as User2Type
		,@intUser3Type as User3Type	
		,ISNULL(@chvOfficialUser1,'') as OfficialUser1
		,ISNULL(@chvOfficialUser2,'') as OfficialUser2
		,ISNULL(@chvOfficialUser3,'') as OfficialUser3
		,'' as DownloadURL
		,ISNULL(QRUrl,'') as QRUrl
	FROM [dbo].[AgentUsers] AS U WITH(NOLOCK)

	WHERE  U.ID = @ID
	--END

	RETURN 1
END

