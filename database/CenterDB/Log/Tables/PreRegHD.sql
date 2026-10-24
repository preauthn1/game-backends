CREATE TABLE [Log].[PreRegHD] (
    [ID]         BIGINT         IDENTITY (1, 1) NOT NULL,
    [IPAddress]  VARCHAR (40)   NOT NULL,
    [HDCode]     VARCHAR (64)   NOT NULL,
    [HDInfo]     VARCHAR (2000) NOT NULL,
    [CreateTime] DATETIME       CONSTRAINT [DF_PreRegHD_CreateTime] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_PreRegHD] PRIMARY KEY CLUSTERED ([ID] ASC)
);

