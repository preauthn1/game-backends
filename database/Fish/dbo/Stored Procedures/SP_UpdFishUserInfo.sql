CREATE PROCEDURE [dbo].[SP_UpdFishUserInfo]
  @in_UserID AS int,
  @in_MaxWeapon AS int ,
  @in_Exp AS int ,
  @in_OnlineTime AS int ,
  @in_HisMaxMoney AS bigint ,
  @in_DayWinMoney AS bigint ,
  @in_DayLoseMoney AS bigint ,
  @in_ShotNum AS int ,
  @in_MissMoneyLib AS int 
AS
BEGIN
	
	update dbo.FishUserInfo set MaxWeaponLevel = @in_MaxWeapon, Exp = Exp + @in_Exp, OnlineTime = OnlineTime + @in_OnlineTime, HisMaxMoney = @in_HisMaxMoney,
	TotalWinMoney = TotalWinMoney + @in_DayWinMoney, TotalLoseMoney = TotalLoseMoney + @in_DayLoseMoney, DayWinMoney = DayWinMoney + @in_DayWinMoney,
	DayLoseMoney = DayLoseMoney + @in_DayLoseMoney, TotalShotNum = TotalShotNum + @in_ShotNum, MissMoneyLib = MissMoneyLib + @in_MissMoneyLib
	where UserID=@in_UserID;
	
END;