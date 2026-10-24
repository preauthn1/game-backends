USE [CenterDB]
GO

/****** Object:  UserDefinedFunction [dbo].[Fn_GetPlatformAccessList]    Script Date: 2019/7/12 下午 05:53:25 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:  Rory
-- Create date: 2019/07/12
-- Description:	回傳具備權限的平台/渠道列表
-- =============================================
CREATE FUNCTION [dbo].[Fn_GetAdminAccessRights]
(
	@intAdminID	INT
)
RETURNS @temp_platformid TABLE (PlatformID int, AreaID int) 
AS
BEGIN

	insert into @temp_platformid
	select	PlatformID, AreaID from [Area].[AreaAdminUser] with (nolock) where AdminID=@intAdminID


	RETURN
END
GO


