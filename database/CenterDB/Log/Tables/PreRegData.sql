CREATE TABLE [Log].[PreRegData] (
    [ID]            BIGINT       IDENTITY (1, 1) NOT NULL,
    [AreaID]        INT          NOT NULL,
    [HDCode]        VARCHAR (64) NOT NULL,
    [IPAddress]     VARCHAR (40) NOT NULL,
    [CreateTime]    DATETIME     CONSTRAINT [DF_PreRegData_CreateTime] DEFAULT (getdate()) NOT NULL,
    [UpdateTime]    DATETIME     CONSTRAINT [DF_PreRegData_ExpireTime] DEFAULT (getdate()) NOT NULL,
    [LastIPAddress] VARCHAR (40) NULL,
    CONSTRAINT [PK_PreRegData] PRIMARY KEY CLUSTERED ([ID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IDX_PreRegData_AreaID_HDCode]
    ON [Log].[PreRegData]([AreaID] ASC, [HDCode] ASC);

