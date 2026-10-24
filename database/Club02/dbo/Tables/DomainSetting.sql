CREATE TABLE [dbo].[DomainSetting] (
    [ID]           INT           IDENTITY (1, 1) NOT NULL,
    [PlatformID]   INT           NOT NULL,
    [AreaID]       INT           NULL,
    [Domain]       VARCHAR (250) NOT NULL,
    [Status]       TINYINT       NOT NULL,
    [CreateTime]   DATETIME      NOT NULL,
    [PlatformName] VARCHAR (50)  NULL,
    CONSTRAINT [PK_DomainSetting_1] PRIMARY KEY CLUSTERED ([ID] ASC)
);

