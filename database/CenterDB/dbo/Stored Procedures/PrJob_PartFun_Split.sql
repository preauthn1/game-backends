-- =============================================
-- Author:  <xxx>
-- Create date: <xxx>
-- Description:	<扩充分区函数>
-- =============================================
CREATE PROCEDURE [dbo].[PrJob_PartFun_Split]
	
AS
BEGIN	

	DECLARE @intLastValue INT,@intCurrentUserID INT
	--扩充用户ID的分区函数,提前做好当前用户ID的20个10W的分区,如：当前最大UserID = 100W,就需要将分区扩充到支持300W
	SELECT TOP 1 
	@intLastValue = CAST(v.value AS INT)	
	FROM sys.partition_range_values AS v 
	LEFT JOIN sys.partition_functions AS f ON v.function_id=f.function_id 
	WHERE f.name = 'Fn_UserID'
	ORDER BY value DESC
	--获取当前最新UserID值
	SELECT TOP 1 @intCurrentUserID = UserID FROM Users.UserInfo WITH(nolock) ORDER BY UserID DESC
		
	WHILE @intLastValue < @intCurrentUserID + 2000000
	BEGIN		
		SET @intLastValue = @intLastValue + 100000			--分区间隔配置 10W
		PRINT @intLastValue
		ALTER PARTITION SCHEME S_UserID NEXT USED [Primary]		
		ALTER PARTITION FUNCTION Fn_UserID() SPLIT RANGE(@intLastValue)								
	END		
	
	
	
	
	DECLARE @dtLastDate DATETIME
	--扩充数据的分区函数,提前做好1个月的分区
	SELECT TOP 1 
	@dtLastDate = CAST(v.value AS DATETime)
	--f.name,v.value
	FROM sys.partition_range_values AS v 
	LEFT JOIN sys.partition_functions AS f ON v.function_id=f.function_id 
	WHERE f.name = 'Fn_Data'
	ORDER BY value DESC
	PRINT CONVERT(VARCHAR,@dtLastDate,120)
	WHILE @dtLastDate < DATEADD(mm,1,GETDATE())
	BEGIN		
		SET @dtLastDate = DATEADD(ww,1,@dtLastDate)				--分区隔间，暂时是配置1周
		PRINT CONVERT(VARCHAR,@dtLastDate,120)
		ALTER PARTITION SCHEME S_Data NEXT USED [Primary]		
		ALTER PARTITION FUNCTION Fn_Data() SPLIT RANGE(@dtLastDate)								
	END		
END

GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrJob_PartFun_Split] TO [xbuser]
    AS [dbo];

