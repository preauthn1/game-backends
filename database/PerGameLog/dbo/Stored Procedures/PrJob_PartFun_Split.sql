-- =============================================
-- Author:		<xxx>
-- Create date: <2016-11-7>
-- Description:	<扩充分区函数>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_PartFun_Split]
	
AS
BEGIN

	
	DECLARE @dtLastDate DATETime
	SELECT TOP 1 
	@dtLastDate = CAST(v.value AS DATETime)
	--f.name,v.value
	FROM sys.partition_range_values AS v 
	LEFT JOIN sys.partition_functions AS f ON v.function_id=f.function_id 
	WHERE f.name = 'Fn_Day'
	ORDER BY value DESC
	PRINT CONVERT(VARCHAR,@dtLastDate,120)
	WHILE @dtLastDate < DATEADD(dd,30,GETDATE())
	BEGIN
		SET @dtLastDate = DATEADD(dd,1,@dtLastDate)		
		ALTER PARTITION SCHEME S_Day NEXT USED [Primary]		
		ALTER PARTITION FUNCTION Fn_Day() SPLIT RANGE(@dtLastDate)								
	END				
	
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_PartFun_Split] TO [xbuser]
    AS [dbo];

