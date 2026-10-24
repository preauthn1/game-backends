CREATE TABLE [Area].[Campaign](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PlatformID] [int] NOT NULL,
	[AreaID] [int] NULL,
	[SeqID] [int] NOT NULL,
	[Picture] [varchar](512) NOT NULL,
	[Title] [nvarchar](100) NOT NULL,
	[Tag] [tinyint] NOT NULL,
	[Behavior] [tinyint] NOT NULL,
	[Link] [varchar](512) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Status] [tinyint] NOT NULL,
	[CreateTime] [datetime] NOT NULL,
 CONSTRAINT [PK_Campaign] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_SeqID]  DEFAULT ((0)) FOR [SeqID]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_Title]  DEFAULT ('') FOR [Title]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_Tag]  DEFAULT ((0)) FOR [Tag]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_Behavior]  DEFAULT ((0)) FOR [Behavior]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_Link]  DEFAULT ('') FOR [Link]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_StartTime]  DEFAULT (getdate()) FOR [StartTime]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_Status]  DEFAULT ((1)) FOR [Status]
GO

ALTER TABLE [Area].[Campaign] ADD  CONSTRAINT [DF_Campaign_CreateTime]  DEFAULT (getdate()) FOR [CreateTime]
GO


