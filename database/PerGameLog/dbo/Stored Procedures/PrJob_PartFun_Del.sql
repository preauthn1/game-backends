-- =============================================
-- Author:		<xxx>
-- Create date: <2016-11-7>
-- Description:	<合并分区函数>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_PartFun_Del]
	
AS
BEGIN
	
		
	DECLARE @dtFirstDate DATETIME
	SELECT TOP 1 
	@dtFirstDate = CAST(v.value AS DATETime)
	--f.name,v.value
	FROM sys.partition_range_values AS v 
	LEFT JOIN sys.partition_functions AS f ON v.function_id=f.function_id 
	WHERE f.name = 'Fn_Day'
	ORDER BY value
	
	WHILE @dtFirstDate < DATEADD(month,-1,GETDATE())
	BEGIN		
		PRINT CONVERT(VARCHAR,@dtFirstDate,120)
		--转移多余数据
		TRUNCATE TABLE PerLog.DetailLogDel
		TRUNCATE TABLE PerLog.PerGameLogDel
		
		ALTER TABLE PerLog.DetailLog SWITCH PARTITION 1 TO PerLog.DetailLogDel PARTITION 1
		ALTER TABLE PerLog.PerGameLog SWITCH PARTITION 1 TO PerLog.PerGameLogDel PARTITION 1			
				
		--合并分区								
		ALTER PARTITION FUNCTION Fn_Day() MERGE RANGE(@dtFirstDate)
		
		SET @dtFirstDate = DATEADD(dd,1,@dtFirstDate)
	END		
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_PartFun_Del] TO [xbuser]
    AS [dbo];

