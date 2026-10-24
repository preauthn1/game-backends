
-- =============================================
-- Author:  <xxxx>
-- Create date: <xxxx>
-- Description:	<新增游戏名称>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_Area_AddGameAliasName]
@intGameID INT,
@chvIconName VARCHAR(50),
@chvIconTitle VARCHAR(50),
@chvSkin varchar(50),
@chvAliasName VARCHAR(50),
@chvErrMsg	NVARCHAR(50) OUTPUT	
AS
BEGIN

	IF EXISTS (select 1 From Area.GameAliasName where AliasName=@chvAliasName AND GameID=@intGameID)
	BEGIN
		SET @chvErrMsg = '操作失败:游戏名称已经存在'
		RETURN -1
	END

	Insert into Area.GameAliasName(GameID,IconName,IconTitle,Skin,AliasName)
	Values(
	@intGameID,
	ISNULL(@chvIconName,'0'),
	ISNULL(@chvIconTitle,'0'),
	ISNULL(@chvSkin,'0'),
	ISNULL(@chvAliasName,'0')
	)

	RETURN 1
END



GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_AddGameAliasName] TO [GameUser]
    AS [dbo];


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrWeb_Area_AddGameAliasName] TO [xbuser]
    AS [dbo];

