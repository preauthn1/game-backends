CREATE TABLE [Area].[AreaCompanyRelations] (
    [AreaID]   INT           NOT NULL,
    [Company]  INT           NOT NULL,
    [CompName] NVARCHAR (30) NULL,
    CONSTRAINT [PK_AreaRelations] PRIMARY KEY CLUSTERED ([AreaID] ASC)
);

