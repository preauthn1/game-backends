CREATE TABLE [Pay].[OrderList] (
    [Id]             INT           IDENTITY (1, 1) NOT NULL,
    [PayTypeID]      INT           NOT NULL,
    [UserID]         INT           NOT NULL,
    [Amount]         BIGINT        NOT NULL,
    [Status]         TINYINT       NOT NULL,
    [IPAddress]      VARCHAR (15)  NOT NULL,
    [TransID]        VARCHAR (32)  NULL,
    [TransData]      VARCHAR (MAX) NOT NULL,
    [CreateTime]     DATETIME      NOT NULL,
    [GWID]           INT           NULL,
    [LastUpdateTime] DATETIME      NULL,
    [ClaimAmount]    BIGINT        CONSTRAINT [DF_OrderList_ClaimAmount_1] DEFAULT ((0)) NOT NULL
) ON [S_Data] ([CreateTime]);


GO
CREATE CLUSTERED INDEX [IX_OrderList_CreateTime]
    ON [Pay].[OrderList]([CreateTime] ASC)
    ON [S_Data] ([CreateTime]);


GO
CREATE NONCLUSTERED INDEX [IDX_OrderList_GWID_PTID_IP_INCLUDE]
    ON [Pay].[OrderList]([GWID] ASC, [PayTypeID] ASC, [IPAddress] ASC)
    INCLUDE([Id], [UserID], [Status], [CreateTime], [TransID], [TransData])
    ON [PRIMARY];


GO
CREATE NONCLUSTERED INDEX [IX_OrderList_Id_UID_STS_INCLUDE]
    ON [Pay].[OrderList]([Id] ASC, [UserID] ASC, [Status] ASC)
    INCLUDE([Amount], [TransID], [TransData], [CreateTime])
    ON [PRIMARY];


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'1:訂單建立 2:訂單完成 3:失敗', @level0type = N'SCHEMA', @level0name = N'Pay', @level1type = N'TABLE', @level1name = N'OrderList', @level2type = N'COLUMN', @level2name = N'Status';

