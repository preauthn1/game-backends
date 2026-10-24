-- =============================================
-- Author:  alan
-- Create date: 2018/8/28
-- Description: 格式化邀请码, 转换大写且置换易搞混的字元
-- =============================================
CREATE FUNCTION [dbo].[Fn_FormatInviteCode]
(
	@chvInviteCode varchar(20)
)
RETURNS varchar(32)
AS
BEGIN

	SET @chvInviteCode = UPPER(@chvInviteCode)
	SET @chvInviteCode = Replace(@chvInviteCode,'O','P')
	SET @chvInviteCode = Replace(@chvInviteCode,'0','K')
	SET @chvInviteCode = Replace(@chvInviteCode,'I','U')
	SET @chvInviteCode = Replace(@chvInviteCode,'1','Q')
		
	RETURN @chvInviteCode
END
