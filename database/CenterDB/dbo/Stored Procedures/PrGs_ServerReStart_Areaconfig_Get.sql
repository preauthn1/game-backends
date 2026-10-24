-- =============================================
-- Author:  jason
-- Create date:2019/05/28
-- Description:	<游戏服务器重新启动后取areaconfig>
--				临时用,待Server改Redis快取之后删除
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_ServerReStart_Areaconfig_Get]
(
	@intAreaID	INT,
    @chvErrMsg VARCHAR(128) OUTPUT
)
AS
BEGIN
	
	SET NOCOUNT ON

	IF NOT EXISTS (SELECT TOP 1 1 FROM [Area].[Area] WITH (NOLOCK) WHERE AreaID = @intAreaID)
	BEGIN
		SET @chvErrMsg = '操作错误 : 无对应渠道编号'
		RETURN -1
	END

	SELECT	AreaID,AreaConfig
	FROM [Area].[Area] WITH (NOLOCK)
	WHERE
		AreaID = @intAreaID

	SET @chvErrMsg = '操作完成'
	RETURN 1
END


GO
GRANT EXECUTE
    ON OBJECT::[dbo].[PrGs_ServerReStart_Areaconfig_Get] TO [GameUser]
    AS [dbo];

