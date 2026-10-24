USE [CenterDB]
GO

IF EXISTS (SELECT * FROM sysobjects WHERE id = object_id('dbo.PrWeb_GetPayOrderByID') AND OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE [dbo].[PrWeb_GetPayOrderByID]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Mars
-- Create date: 2019/07/09
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE dbo.PrWeb_GetPayOrderByID
	@intOrderID int = 0,
	@chvRtnMsg varchar(50) = '' OUTPUT	--~T
AS
BEGIN
	SET NOCOUNT ON;

    SELECT ID,
			PayTypeID,
			UserID,
			Amount,
			Status,
			IPAddress,
			TransID,
			TransData,
			CreateTime,
			GWID
	FROM Pay.OrderList 
	WHERE Id = @intOrderID

	SET @chvRtnMsg = '';
	RETURN 1;
END
GO
