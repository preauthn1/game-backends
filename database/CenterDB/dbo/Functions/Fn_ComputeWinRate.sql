-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<计算胜率>
-- =============================================
CREATE FUNCTION [dbo].[Fn_ComputeWinRate] 
(
	@intWinCount INT,
	@intLoseCount INT,
	@intDrawCount INT
)
RETURNS DECIMAL(18, 2)
AS
BEGIN
	IF @intWinCount + @intLoseCount + @intDrawCount = 0
	RETURN 0
	
	RETURN @intWinCount * 1.0 / (@intWinCount + @intDrawCount + @intLoseCount)

END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ComputeWinRate] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[Fn_ComputeWinRate] TO [xbuser]
    AS [dbo];

