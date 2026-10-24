CREATE TABLE [Stat].[WebFunctionType] (
    [TypeID]     SMALLINT      NOT NULL,
    [FuncID]     TINYINT       NOT NULL,
    [RelationID] TINYINT       CONSTRAINT [DF_WebFunctionType_RelationID] DEFAULT ((0)) NOT NULL,
    [FuncName]   NVARCHAR (16) NOT NULL,
    [CreateTime] DATETIME      CONSTRAINT [DF_WebFunctionType_CreateTime] DEFAULT (getdate()) NOT NULL,
    [Memo]       VARCHAR (50)  NULL,
    CONSTRAINT [PK_WebFunctionType] PRIMARY KEY CLUSTERED ([TypeID] ASC, [FuncID] ASC)
);

