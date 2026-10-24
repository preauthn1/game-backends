CREATE TABLE [Game].[JackpotPool_Link](
	[GameID] [int] NOT NULL,
	[LevelID] [int] NOT NULL,
	[Contribution] [bigint] NOT NULL,
	[TotalContribution] [bigint] NOT NULL,
	[AwardTotal] [bigint] NOT NULL,
	[FlagID] [int] NOT NULL,
 CONSTRAINT [PK_JackpotPool_Link_1] PRIMARY KEY CLUSTERED 
(
	[GameID] ASC,
	[LevelID] ASC,
	[FlagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Game].[JackpotPool_Link] ADD  DEFAULT ((0)) FOR [FlagID]
GO


