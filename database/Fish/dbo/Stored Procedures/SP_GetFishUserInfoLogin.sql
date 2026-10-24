CREATE PROCEDURE [dbo].[SP_GetFishUserInfoLogin]
  @in_UserID AS int 
AS
BEGIN
	DECLARE @_Count int;
	DECLARE @CurrentDate int;
	
	DECLARE @MaxWeaponLevel int;
	DECLARE @Exp int;
	DECLARE @OnlineTime int;
	DECLARE @HisMaxMoney int;
	DECLARE @TotalWinMoney int;
	DECLARE @TotalLoseMoney int;
	DECLARE @DayWinMoney int;
	DECLARE @DayLoseMoney int;
	DECLARE @TotalShotNum int;
	DECLARE @MissMoneyLib int;
	DECLARE @LastLoginTime int;

	
	SELECT @_Count=COUNT(1) FROM dbo.FishUserInfo where UserID=@in_UserID;
	if(@_Count = 0) 
	BEGIN
		insert into dbo.FishUserInfo (UserID) values(@in_UserID);
	END
	
	select top 1 @MaxWeaponLevel = MaxWeaponLevel, @Exp = Exp, @OnlineTime = OnlineTime, @HisMaxMoney = HisMaxMoney, @TotalWinMoney = TotalWinMoney, 
			@TotalLoseMoney = TotalLoseMoney, @DayWinMoney = DayWinMoney, @DayLoseMoney = DayLoseMoney, @TotalShotNum = TotalShotNum, 
			@MissMoneyLib = MissMoneyLib, @LastLoginTime = CAST(CONVERT(varchar, LastLoginTime, 112) AS INT)
	from dbo.FishUserInfo 
	where UserID=@in_UserID;
	
	SET @CurrentDate = CAST(CONVERT(varchar, GETDATE(), 112) AS INT );
	if(@CurrentDate != @LastLoginTime)
	BEGIN
		SET @DayWinMoney = 0;
		SET @DayLoseMoney = 0;
		UPDATE dbo.FishUserInfo set DayWinMoney = 0, DayLoseMoney = 0 where UserID=@in_UserID;
	END
	UPDATE fishuserinfo set LastLoginTime = GETDATE() WHERE UserID=@in_UserID;

	select @MaxWeaponLevel as MaxWeaponLevel, @Exp as Exp, @OnlineTime as OnlineTime, @HisMaxMoney as HisMaxMoney, @TotalWinMoney as TotalWinMoney, 
			@TotalLoseMoney as TotalLoseMoney, @DayWinMoney as DayWinMoney, @DayLoseMoney as DayLoseMoney, @TotalShotNum as TotalShotNum, 
			@MissMoneyLib as MissMoneyLib, @LastLoginTime as LastLoginTime;

END;