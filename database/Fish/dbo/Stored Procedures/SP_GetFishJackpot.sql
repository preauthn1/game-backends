
CREATE PROCEDURE [dbo].[SP_GetFishJackpot]
  @in_ServerID AS int 
AS
BEGIN
	
	select top 1 ServerID, ShotID, ShotNum, ShotBet, FishMoney, Win, CommissionMoney, LineNum, LineBase, LineUp, LineDown, PowerBase, 
			PowerAdd, Power, AvgBetTotal, ShotTotal, ShotBetCalc, FishMoneyTotal, WinTotal, ShotBetTotal, CommissionTotal, InitCalc
	from dbo.FishJackpot where ServerID = @in_ServerID;
	
END;