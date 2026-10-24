-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<计算用户等级>
--	2018/05/10	Jason	#87 依運營需求調整等級判斷 @lngTaxAmount改@lngChargeAmount計算	
-- =============================================
CREATE FUNCTION [dbo].[Fn_ComputeGrade]
(
	@lngChargeAmount	BIGINT
)
RETURNS INT
AS
BEGIN
	
	DECLARE @intGrade INT
	
	SELECT TOP 1 @intGrade = Id FROM Users.GradeSetting WHERE @lngChargeAmount >= Amount ORDER BY Id DESC
	
	RETURN ISNULL(@intGrade,0)

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ComputeGrade] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ComputeGrade] TO [xbuser]
    AS [dbo];

