-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_User_CSOnBoard]
	@intOperatorID	int,
	@intStatus		int,			--0:上班;1:下班
	@chvErrMsg		varchar(100)	OUTPUT
AS
BEGIN

	MERGE [CenterDB].[Users].[CSOnBoard] AS T
	USING (SELECT UserID = @intOperatorID,
				  Status = @intStatus) AS S
	  ON T.UserID = S.UserID 
	WHEN MATCHED THEN
		 UPDATE 
		    SET [Status] = @intStatus,
		        CreateTime = CASE WHEN @intStatus = 0 THEN GETDATE() ELSE CreateTime END,
				UpdateTime = CASE WHEN @intStatus = 1 THEN GETDATE() ELSE UpdateTime END
	WHEN NOT MATCHED AND @intStatus = 0 THEN
		INSERT (UserID, [Status], CreateTime, UpdateTime)
		VALUES (@intOperatorID, [Status], GETDATE(),  GETDATE());
	
	return 1
END
