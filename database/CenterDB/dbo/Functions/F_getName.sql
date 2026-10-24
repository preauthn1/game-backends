-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<获取IP区域>
-- =============================================
CREATE FUNCTION [dbo].[F_getName]
(
	@chvSName varchar(255)	
)
RETURNS varchar(100)
AS
BEGIN
	declare @chvName varchar(100)
	
	SELECT @chvName = AreaName FROM area.city WHERE  @chvSName like '%'+AreaName+'%'
	 or (AreaName like '%市' AND @chvSName like '%'+substring(AreaName,1,LEN(areaname)-1)+'%')
	if @chvName IS NULL
	BeGIN
		SELECT @chvName = AreaName FROM area.State WHERE  @chvSName like '%'+AreaName+'%'
	 or (AreaName like '%省' AND @chvSName like '%'+substring(AreaName,1,LEN(areaname)-1)+'%')
	END
	return @chvName
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[F_getName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[F_getName] TO [xbuser]
    AS [dbo];

