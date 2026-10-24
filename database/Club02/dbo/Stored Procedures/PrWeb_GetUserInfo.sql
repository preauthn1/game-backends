-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetUserInfo]
@intUserID			INT,					--用户ID
@chvErrMsg			VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN
	SELECT LoginName = CASE WHEN UserType = 3 THEN '' ELSE LoginName END, 
	       UnionpayRealName, 
	       UnionpayBank, 
	       UnionpayAccount
	  FROM CenterDB.Users.UserInfo 
	 WHERE UserID = @intUserID


	return 1
END