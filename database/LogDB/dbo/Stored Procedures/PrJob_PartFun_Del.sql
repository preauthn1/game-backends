
-- =============================================
-- Author:		<xxx>
-- Create date: <2018-3-5>
-- Description:	<合并分区函数>
-- =============================================
Create PROCEDURE [dbo].[PrJob_PartFun_Del]
	
AS
BEGIN
	
		
	DECLARE @dtFirstDate DATETIME
	SELECT TOP 1 
	@dtFirstDate = CAST(v.value AS DATETime)
	--f.name,v.value
	FROM sys.partition_range_values AS v 
	LEFT JOIN sys.partition_functions AS f ON v.function_id=f.function_id 
	WHERE f.name = 'Fn_Data'
	ORDER BY value
	
	WHILE @dtFirstDate < DATEADD(month,-1,GETDATE())
	BEGIN		
		PRINT CONVERT(VARCHAR,@dtFirstDate,120)
				
		--合并分区								
		ALTER PARTITION FUNCTION Fn_Data() MERGE RANGE(@dtFirstDate)
		
		SET @dtFirstDate = DATEADD(ww,1,@dtFirstDate)
	END		
END
GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_PartFun_Del] TO [xbuser]
    AS [dbo];

