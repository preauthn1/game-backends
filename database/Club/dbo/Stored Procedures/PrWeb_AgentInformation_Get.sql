


-- =============================================
-- Author:  harry
-- Create date: 2018/08/15
-- Description:	查询操作者个人资讯
--
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_AgentInformation_Get]
	@AgentUID		uniqueidentifier
AS
BEGIN
	SELECT  U.UserID, U.LoginName, U.Tel, U.Weixin, U.QQ, U.Email
	, U.QRUrl, U.Class, U.InviteCode, U.AllowWithdraw,
	 P.Name AS PlatformName, A.AreaName, G.GradeName, G.DistributeRate, U.TotalFlows, U.TotalIncome, U.RemainAmount,
	 C.PlatformID,
	 C.User1Type,C.User2Type,C.User3Type,C.OfficialUser1,C.OfficialUser2,C.OfficialUser3
	FROM AgentUsers U with (nolock) 
	LEFT JOIN CenterDB.Area.Area A with (nolock) ON A.AreaID = U.AreaID 
	LEFT JOIN CenterDB.Area.Platform P with (nolock) ON P.PlatformID = U.PlatformID
	LEFT JOIN GrandConfig G with (nolock) ON G.AgentID = U.ID AND G.Level = U.Level
	LEFT JOIN OfficialUserConfig C with (nolock) on C.PlatformID = U.PlatformID
	WHERE U.ID = @AgentUID
END



