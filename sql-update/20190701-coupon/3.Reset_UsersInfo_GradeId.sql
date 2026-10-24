USE [CenterDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Mars.Tseng
-- Create date: <2019-06-19 03:50>
-- Description:	重置玩家VIP等級
-- =============================================
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
	
    -- Insert statements for procedure here
	DECLARE @intId				INT,
			@intAmount			INT;

	DECLARE cur CURSOR READ_ONLY 
	FOR 
	SELECT Id, Amount FROM Users.GradeSetting ORDER BY Id ASC

	OPEN cur

	FETCH NEXT FROM cur
	INTO @intId, @intAmount 

	WHILE @@FETCH_STATUS = 0
	BEGIN
		UPDATE Users.UserInfo 
		SET GradeID = @intId 
		WHERE (SumCharge + SumAgentCharge) >= @intAmount

		FETCH NEXT FROM cur INTO @intId, @intAmount 
	END

	CLOSE cur
	DEALLOCATE cur
END
GO