-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<统计等级人数>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Stat_GetGradeUserCount]
	
AS
BEGIN
	
	
	SELECT G.Id,G.Amount,ISNULL(A.UserCount,0) as UserCount FROM 
	Users.GradeSetting as G
	LEFT JOIN 
	(
		select GradeID,COUNT(1) as UserCount from Users.UserInfo with(nolock) 
		group by GradeID
	) as A ON G.Id = A.GradeID
	
	
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGradeUserCount] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Stat_GetGradeUserCount] TO [xbuser]
    AS [dbo];

