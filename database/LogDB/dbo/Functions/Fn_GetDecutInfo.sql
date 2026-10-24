-- =============================================
-- Author:		<xxx>
-- Create date: <2016-9-6>
-- Description:	<获取用户退出桌子时的扣款信息>
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetDecutInfo]
(	
	@chvDeductInfo		VARCHAR(max)
)
RETURNS @returnTable TABLE (UserID int,[Amount] int)
BEGIN 		

	DECLARE @pos INT
	
	SET @pos = CHARINDEX(',',@chvDeductInfo)
	WHILE @pos>0 OR LEN(@chvDeductInfo)>0
	BEGIN
		DECLARE @r VARCHAR(max)
		IF @pos=0
		BEGIN
			SET @r = @chvDeductInfo
			SET @chvDeductInfo = ''	
		END
		ELSE
		BEGIN
			SET @r = SUBSTRING(@chvDeductInfo,0,@pos)
			SET @chvDeductInfo = SUBSTRING(@chvDeductInfo,@pos+1,LEN(@chvDeductInfo))
		END 
		
		DECLARE @intUserID INT,@intAmount INT
		SET @intUserID = CAST(SUBSTRING(@r,0,CHARINDEX(':',@r)) AS INT)
		SET @intAmount = CAST(SUBSTRING(@r,CHARINDEX(':',@r)+1,LEN(@r)) AS INT)
		
		INSERT INTO @returnTable
		SELECT @intUserID,@intAmount				
		
		SET @pos = CHARINDEX(',',@chvDeductInfo)	
	END
	RETURN 
END

GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetDecutInfo] TO [GameUser]
    AS [dbo];


GO
GRANT SELECT
    ON OBJECT::[dbo].[Fn_GetDecutInfo] TO [xbuser]
    AS [dbo];

