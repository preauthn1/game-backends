CREATE TABLE [Area].[PlatformDownload] (
    [PlatformID]   INT           NOT NULL,
    [IOS]          VARCHAR (256) NOT NULL,
    [Android]      VARCHAR (256) NOT NULL,
    [Official]     VARCHAR (256) NOT NULL,
    [IOSTerminate] TINYINT       NOT NULL,
    [IOSMemo]      VARCHAR (256) NULL,
    [Correct]      TINYINT       NOT NULL,
    [UpdateTime]   DATETIME      CONSTRAINT [DF_PlatformDownload_CreateTime] DEFAULT (getdate()) NOT NULL,
    [Memo]         VARCHAR (256) CONSTRAINT [DF_PlatformDownload_Memo] DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_PlatformDownload] PRIMARY KEY CLUSTERED ([PlatformID] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'IOS下載頁', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'PlatformDownload', @level2type = N'COLUMN', @level2name = N'IOS';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'安卓下載頁', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'PlatformDownload', @level2type = N'COLUMN', @level2name = N'Android';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'官方下載頁', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'PlatformDownload', @level2type = N'COLUMN', @level2name = N'Official';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'IOS是否下架 1 已經下架 0上架', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'PlatformDownload', @level2type = N'COLUMN', @level2name = N'IOSTerminate';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'是否正確 1正確 0不正確', @level0type = N'SCHEMA', @level0name = N'Area', @level1type = N'TABLE', @level1name = N'PlatformDownload', @level2type = N'COLUMN', @level2name = N'Correct';

