USE [CenterDB]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- 新增Users.GradeDetail來記錄玩家目前獲得晉級的獎勵位置 -----------------------------------
IF not exists (SELECT * FROM sysobjects WHERE id = object_id(N'Users.GradeDetail') and OBJECTPROPERTY(id, N'IsUserTable') = 1) 
BEGIN
	CREATE TABLE [Users].[GradeDetail](
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[UserID] [int] NOT NULL,
		[GradeID] [int] NOT NULL,
		[IPAddress] [nchar](50) NOT NULL,
		[CreateTime] [datetime] NOT NULL,
		[LastUpdateTime] [datetime] NULL,
		PRIMARY KEY CLUSTERED 
		(
			[Id] ASC
		)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]
END

------------------------------------------------------------------------------------------
-- 新增Table - PromotionAward欄位存放晉級VIP獎勵 -------------------------------------------------
IF COL_LENGTH('Users.GradeSetting', 'PromotionAward') IS NULL
BEGIN
	ALTER TABLE [Users].[GradeSetting] ADD PromotionAward INT NULL DEFAULT 0;
END
GO

-- 新增Table - AvatarFrame欄位存放VIP頭框ID -------------------------------------------------
IF COL_LENGTH('Users.GradeSetting', 'AvatarFrame') IS NULL
BEGIN
	ALTER TABLE [Users].[GradeSetting] ADD AvatarFrame INT NULL DEFAULT 1;
END
GO

-- 刪除現有GradeSetting並重新設定 --------------------------------------------------------
DELETE FROM [Users].[GradeSetting];
GO

-- 增加新設定 ---------------------------------------------------------------------
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (1, 0, 0, 1);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (2, 10000, 600, 2);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (3, 30000, 1800, 3);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (4, 100000, 5800, 4);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (5, 1000000, 13800, 5);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (6, 5000000, 58800, 6);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (7, 20000000, 188800, 7);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (8, 50000000, 388800, 8);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (9, 100000000, 888800, 9);
INSERT INTO [Users].[GradeSetting] (Id, Amount, PromotionAward, AvatarFrame) VALUES (10, 500000000, 3888800, 10);
GO

-- 增加 AvatarFrame 欄位  ---------------------------------------------------------------------
IF COL_LENGTH('Users.userinfo', 'AvatarFrame') IS NULL
BEGIN
	ALTER TABLE [Users].[UserInfo] ADD AvatarFrame INT NULL DEFAULT 1;
END
GO

-- 修改GradeID欄位 default  ---------------------------------------------------------------------
IF COL_LENGTH('Users.userinfo', 'AvatarFrame') IS NOT NULL
BEGIN
	ALTER TABLE [Users].[UserInfo] DROP CONSTRAINT [DF_UserInfo_GradeID];
	ALTER TABLE [Users].[UserInfo] ADD  CONSTRAINT [DF_UserInfo_GradeID]  DEFAULT ((1)) FOR [GradeID];
END
GO