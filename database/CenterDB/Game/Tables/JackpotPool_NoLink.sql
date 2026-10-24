CREATE TABLE [Game].[JackpotPool_NoLink](
	[GameID] [int] NOT NULL,
	[TableNo] [int] NOT NULL,
	[LevelID] [int] NOT NULL,
	[Contribution] [bigint] NOT NULL,
	[TotalContribution] [bigint] NOT NULL,
	[AwardTotal] [bigint] NOT NULL,
	[FlagID] [int] NOT NULL,
 CONSTRAINT [PK_JackpotPool_NoLink_1] PRIMARY KEY CLUSTERED 
(
	[GameID] ASC,
	[TableNo] ASC,
	[LevelID] ASC,
	[FlagID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [Game].[JackpotPool_NoLink] ADD  DEFAULT ((0)) FOR [FlagID]
GO