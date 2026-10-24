USE [CenterDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:  mars
-- Create date: 2019/7/02
-- Description:	提供服務器進入遊戲房間,获取用户基本信息
-- Changelog:
-- =============================================
CREATE PROCEDURE [dbo].[PrPs_Pay_PromotionAward]
	@intUserID			INT,						--用户ID
	@chvIPAddress		VARCHAR(40) = '0.0.0.0',	--IP
	@chvErrMsg			VARCHAR(128) OUTPUT			--返回消息
AS
BEGIN 
	DECLARE @NowTime			DATETIME = GETDATE(), --現在時間
			@UserID				INT = @intUserID, --UserId
			@UserInfoGradeID	INT, -- UserInfo.GradeID
			@UserDetailGradeID	INT; -- UserDetail.GradeID
	
	/*  Get Users.UserInfo GradeID  */
	------------------------------------------------------------------------------------------
	SELECT TOP 1 @UserInfoGradeID = GradeID FROM Users.UserInfo WHERE UserId = @intUserID
	------------------------------------------------------------------------------------------
	
	/*  Get Users.GradeDetail GradeID  */
	------------------------------------------------------------------------------------------
	SELECT TOP 1 @UserDetailGradeID = GradeID FROM Users.GradeDetail WHERE UserId = @intUserID
	IF @@ROWCOUNT = 0
	BEGIN
		SET @UserDetailGradeID = 0
		/* 新增資料行Users.GradeDetail */
		INSERT INTO Users.GradeDetail (UserID, GradeID, IPAddress, CreateTime) VALUES (@intUserID, @UserDetailGradeID, 1, getdate())
	END
	------------------------------------------------------------------------------------------
	
	IF (@UserInfoGradeID > @UserDetailGradeID)
	/*  pay PromotionAward & send mail  */
	------------------------------------------------------------------------------------------
	BEGIN
		DECLARE @lngUpdateAmount	INT = 0, 						--晉級獎勵
				@sintSourceType		INT = 10004, 					--更新方式::晋级VIP奖励
				@chvMemo			VARCHAR(20) = '晋级VIP奖励',	--说明
				@chvTitle			VARCHAR(100) = '恭喜您提升VIP等级!!',
				@lngCurrentAmount	BIGINT;
	
		-- 取得未發送獎額總額
		SELECT @lngUpdateAmount = ISNULL(SUM(PromotionAward), 0) FROM Users.GradeSetting WHERE Id <= @UserInfoGradeID AND Id > @UserDetailGradeID
	
		IF(@lngUpdateAmount <= 0)
		BEGIN
			RETURN -1
		END
	
		-- 更新保險箱金額
		EXEC [dbo].[PrPsWeb_UpdateUserBank]
			@intUserID,				--用户ID
			@sintSourceType,		--更新方式
			@lngUpdateAmount,		--更新的数量
			@chvIPAddress,			--IP地址
			@chvMemo,				--说明
			NULL,					--返回当前银行金币数
			Null
	
		--发送晉級獎勵邮件
		DECLARE @chvMailContent varchar(1024)
		SET @chvMailContent = '这是您的奖金' + dbo.Fn_ShowMoney(@lngUpdateAmount) + '元，请在保险箱领取'
	
		--发送系统邮件
		EXEC PrPs_SendSysMail
			@intUserID, 
			@chvTitle,
			@chvMailContent,
			0,
			NULL
	
		--更新GradeDetail
		UPDATE Users.GradeDetail SET GradeID = @UserInfoGradeID WHERE UserId = @intUserID	
		RETURN 1
	
	END
END