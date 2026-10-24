


CREATE PROCEDURE [dbo].[SP_UpdFishUserMaxWeaponLevel]
  @in_UserID AS int ,
  @in_MaxWeaponLevel AS int 
AS
BEGIN

	update dbo.FishUserInfo set MaxWeaponLevel = @in_MaxWeaponLevel where UserID=@in_UserID and MaxWeaponLevel<@in_MaxWeaponLevel;
	
END;