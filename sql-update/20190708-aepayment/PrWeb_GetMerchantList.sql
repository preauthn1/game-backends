USE [CenterDB]
GO

/****** Object:  StoredProcedure [dbo].[PrWeb_GetMerchantList]    Script Date: 2019/7/8 U 11:21:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Mars
-- Create date: 2019/07/07
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[PrWeb_GetMerchantList] 
	@chvGWCode varchar(50),
	@chvRtnMsg	varchar(50) = '' OUTPUT	--~T
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @GWID int = null,
			@GWName varchar(50),
			@GWInfo int = 0;

	SELECT @GWID = ID, @GWName = GatewayName, @GWInfo = MerchantInfo FROM Payment.PayGateway WHERE GatewayCode = @chvGWCode;

	IF @@ROWCOUNT = 0
	BEGIN 
		SET @chvRtnMsg = 'dL'
		RETURN -1
	END
	
	SELECT @GWName as GWName, @GWID as GWID, @GWInfo as Info, ID, InfoData, uuid, MerchantStatus, Status 
	FROM Payment.MerchantInfo 
	WHERE GWID = @GWID;

	SET @chvRtnMsg = ''
	RETURN 1
END
GO


