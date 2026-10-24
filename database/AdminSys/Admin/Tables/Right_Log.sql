CREATE TABLE [Admin].[Right_Log] (
    [LogID]      INT           IDENTITY (1, 1) NOT NULL,
    [RightID]    INT           NOT NULL,
    [RightName]  VARCHAR (32)  NOT NULL,
    [ParentID]   INT           NOT NULL,
    [RightToken] VARCHAR (128) NOT NULL,
    [PageUrl]    VARCHAR (512) NULL,
    [RightType]  TINYINT       NOT NULL,
    [Sort]       INT           NOT NULL,
    [ShowInMenu] BIT           NOT NULL,
    [IconCls]    VARCHAR (32)  NULL,
    CONSTRAINT [PK_Right_Log] PRIMARY KEY CLUSTERED ([LogID] ASC)
);

