**CREATE TABLE Appointments

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Appointments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[DogName] [nvarchar](max) NOT NULL,
	[Date] [datetime2](7) NOT NULL,
	[Username] [nvarchar](max) NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[DogSize] [nvarchar](max) NOT NULL,
	[DurationMinutes] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Appointments] ADD  CONSTRAINT [PK_Appointments] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Appointments] ADD  DEFAULT ('0001-01-01T00:00:00.0000000') FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Appointments] ADD  DEFAULT (N'') FOR [DogSize]
GO
ALTER TABLE [dbo].[Appointments] ADD  DEFAULT ((0)) FOR [DurationMinutes]
GO
ALTER TABLE [dbo].[Appointments] ADD  DEFAULT ((0.0)) FOR [Price]
GO




**CREATE TABLE Users


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](max) NOT NULL,
	[Password] [nvarchar](max) NOT NULL,
	[FullName] [nvarchar](max) NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO



**CREATE VIEW 

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[vw_AppointmentsWithUsers] AS
SELECT
    a.Id,
    a.Username,
    u.FullName AS FullName,
    a.DogName,
    a.DogSize,
    a.Date,
    a.CreatedAt,
    a.Price,
    a.DurationMinutes
FROM dbo.Appointments AS a
INNER JOIN dbo.Users AS u ON u.Username = a.Username;
GO




**CREATE PROCEDURE

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_GetUserLoyaltyPreview]
    @Username NVARCHAR(450)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @cnt INT = (
        SELECT COUNT(*)
        FROM dbo.Appointments
        WHERE Username = @Username
    );
    -- Next booking will be 0-based index @cnt; 10% only from 4th appointment (@cnt >= 3).
    DECLARE @minExistingBeforeNextGetsLoyalty INT = 3;
    SELECT
        @cnt AS AppointmentCount,
        CASE WHEN @cnt >= @minExistingBeforeNextGetsLoyalty THEN 10 ELSE 0 END AS NextBookingDiscountPercent;
END
GO
