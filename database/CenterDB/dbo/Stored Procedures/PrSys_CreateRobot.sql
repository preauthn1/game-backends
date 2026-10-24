-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<生成机器人>
--	2018/5/2 alan	userID 改為 直接取號 
--	2018/5/8 Jason	RobotList新增Group AND Sort寫入 
-- =============================================
 CREATE PROCEDURE [dbo].[PrSys_CreateRobot]
	@lngMinCashAmount	BIGINT,
	@lngMaxCashAmount	BIGINT,
	@tnyMinLevel	int,
	@tnyMaxLevel	int,
	@intCount		int,
	@chvGroup		char(1)
AS
BEGIN
	




--创建账号
while @intCount>0
begin			
	declare @chvLoginName varchar(32),@chvNickName varchar(32),@chvPassword varchar(32),@intFaceID int,@tnySex tinyint,@intUserID int

	set @chvLoginName = (select top 1 value from dbo.fn_split('13,15,17,18',',') ORDER BY NEWID())
	set @chvLoginName = @chvLoginName + cast(round(RAND()*(2-1),0)+1 as varchar) + cast(round(RAND()*(9999-1000),0)+1000 as varchar)++ cast(round(RAND()*(9999-1000),0)+1000 as varchar)

	WHILE EXISTS(select 1 from users.UserInfo with(nolock) where LoginName=@chvLoginName)
	BEGIN
		set @chvLoginName = @chvLoginName + cast(round(RAND()*(2-1),0)+1 as varchar) + cast(round(RAND()*(9999-1000),0)+1000 as varchar)++ cast(round(RAND()*(9999-1000),0)+1000 as varchar)
	END

	--生成昵称
	EXEC dbo.PrPsWeb_CreateNickName
	2,
	@chvNickName OUTPUT

	--生成密码
	SET @chvPassword = [REDACTED], '-', ''))

	--生成头像ID
	SET @intFaceID = CAST(RAND() * 16 + 1 AS INT)

	--生成性别
	SET @tnySex = CASE WHEN @intFaceID<9 THEN 0 ELSE 1 END

	--取得隨機IP
	declare @intIP bigint
	select top 1 @intIP = ipMin from Area.IP with (nolock) order by NEWID()
	declare @ip varchar(20);
	select @ip=dbo.f_Int2IP(@intIP);




	/* 直接取號  */
	-----------------------------------------------------------------------------------
	EXEC [dbo].[PrPs_User_ApplyNewUID_S]
		@intUserID	= @intUserID	OUTPUT

	SET @intUserID	= ISNULL(@intUserID,0)
	-----------------------------------------------------------------------------------


	--创建账号
	INSERT INTO Users.UserInfo
			( [UserID],
			  FaceID,
			  LoginName ,
			  NickName ,
			  Password ,
			  BankPassword,
			  Sex ,
			  HDType ,
			  HDCode ,
			  UserType,
			  CreateTime ,
			  CreateIPAddress ,
			  LastLogonTime ,
			  LastLogonIPAddress,
			  AreaID
			)
	SELECT 
			@intUserID,
			@intFaceID,
			@chvLoginName,
			@chvNickName ,
			@chvPassword,
			dbo.Fn_EncryptPwd('888888'),
			@tnySex,
			1 as HDType,
			'robot' as HDCode,
			2,
			GETDATE(),
			@ip,
			GETDATE(),
			@ip,
			2

	--insert into LogDB.users.UserRegLog (PlatformID,AreaID,UserID,CreateTime,IPAddress,HDType,HDCode)
	--values(2, 2, @intUserID, '2018-05-01', @ip, 1, 'robot')


	declare @lngAmount bigint
	set @lngAmount  = round(RAND()*(@lngMaxCashAmount-@lngMinCashAmount),0)+@lngMinCashAmount
	insert into Money.Cash
	select @intUserID,@lngAmount,''

	insert into Robot.RobotList
		(	
			[UserID],
			[ServerID],
			[LevelID],
			[Group],
			[Sort]
		)
	select top 1
			@intUserID,
			0,
			ROUND(RAND()*(@tnyMaxLevel-@tnyMinLevel),0)+@tnyMinLevel,
			@chvGroup,
			ABS(CHECKSUM(NEWID()))

	
	
	SET @intCount = @intCount - 1
end


	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_CreateRobot] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrSys_CreateRobot] TO [xbuser]
    AS [dbo];

