CREATE TABLE [Admin].[Right] (
    [RightID]    INT           IDENTITY (368, 1) NOT NULL,
    [RightName]  VARCHAR (32)  NOT NULL,
    [ParentID]   INT           NOT NULL,
    [RightToken] VARCHAR (128) NOT NULL,
    [PageUrl]    VARCHAR (512) NULL,
    [RightType]  TINYINT       NOT NULL,
    [Sort]       INT           NOT NULL,
    [ShowInMenu] BIT           NOT NULL,
    [IconCls]    VARCHAR (32)  NULL,
    CONSTRAINT [PK_Right] PRIMARY KEY CLUSTERED ([RightID] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IUX_Right_RightToken]
    ON [Admin].[Right]([RightToken] ASC);

