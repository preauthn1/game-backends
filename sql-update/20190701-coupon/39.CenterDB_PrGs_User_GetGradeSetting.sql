USE [CenterDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  mars
-- Create date: 2019/7/02
-- Description:	提供服務器獲取VIP等級設定
-- Changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrGs_User_GetGradeSetting]
	@chvErrMsg		VARCHAR(128) OUTPUT		--返回消息
AS
BEGIN 

	SELECT Id, Amount, PromotionAward, AvatarFrame FROM Users.GradeSetting
	
	IF @@ROWCOUNT > 0
	BEGIN
		SET @chvErrMsg = '';
		RETURN 1
	END

	ELSE
	BEGIN
		SET @chvErrMsg = '未设定 VIP 层级';
		RETURN -1
	END

END