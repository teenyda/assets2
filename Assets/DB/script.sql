SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[locale]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[locale](
	[locale_id] [int] NOT NULL,
	[locale_name] [nvarchar](50) NULL,
	[locale_type] [int] NULL,
	[locale_state] [int] NULL,
	[locale_explain] [nvarchar](50) NULL,
 CONSTRAINT [PK_locale] PRIMARY KEY CLUSTERED 
(
	[locale_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[brand]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[brand](
	[brand_id] [int] NOT NULL,
	[brand_code] [nchar](10) NULL,
	[brand_name] [nvarchar](50) NULL,
	[brand_state] [int] NULL,
 CONSTRAINT [PK_brand] PRIMARY KEY CLUSTERED 
(
	[brand_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[department]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[department](
	[department_id] [int] NOT NULL,
	[department_code] [nchar](10) NULL,
	[department_state] [int] NULL,
	[department_name] [nvarchar](50) NULL,
 CONSTRAINT [PK_department] PRIMARY KEY CLUSTERED 
(
	[department_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[property_class]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[property_class](
	[pc_id] [int] NOT NULL,
	[pc_code] [nchar](10) NULL,
	[pc_name] [nvarchar](50) NULL,
	[pc_state] [int] NULL,
 CONSTRAINT [PK_property_class] PRIMARY KEY CLUSTERED 
(
	[pc_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[supplier]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[supplier](
	[supplier_id] [int] NOT NULL,
	[supplier_contacts] [nvarchar](50) NULL,
	[supplier_type] [int] NULL,
	[supplier_state] [int] NULL,
	[supplier_name] [nvarchar](50) NULL,
	[supplier_phone] [char](11) NULL,
	[supplier_address] [nvarchar](50) NULL,
 CONSTRAINT [PK_supplier] PRIMARY KEY CLUSTERED 
(
	[supplier_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[property]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[property](
	[property_id] [int] NOT NULL,
	[property_code] [nchar](10) NULL,
	[property_name] [nvarchar](50) NULL,
	[property_date] [datetime] NULL,
	[create_date] [datetime] NULL,
	[locale_id] [int] NULL,
	[supplier_id] [int] NULL,
	[department_id] [int] NULL,
	[property_state] [int] NULL,
	[property_descr] [nvarchar](max) NULL,
	[scrap_way] [int] NULL,
	[pc_id] [int] NULL,
	[brand_id] [int] NULL,
 CONSTRAINT [PK_property] PRIMARY KEY CLUSTERED 
(
	[property_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[user]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[user](
	[user_id] [int] NOT NULL,
	[user_alias] [nvarchar](50) NULL,
	[user_name] [nvarchar](50) NULL,
	[user_password] [nvarchar](50) NULL,
	[user_code] [nvarchar](50) NULL,
	[user_phone] [char](11) NULL,
	[user_sex] [nchar](10) NULL,
	[department_id] [int] NULL,
	[user_job] [nvarchar](50) NULL,
 CONSTRAINT [PK_user] PRIMARY KEY CLUSTERED 
(
	[user_id] ASC
)WITH (PAD_INDEX  = OFF, IGNORE_DUP_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_property_brand]') AND parent_object_id = OBJECT_ID(N'[dbo].[property]'))
ALTER TABLE [dbo].[property]  WITH CHECK ADD  CONSTRAINT [FK_property_brand] FOREIGN KEY([brand_id])
REFERENCES [dbo].[brand] ([brand_id])
GO
ALTER TABLE [dbo].[property] CHECK CONSTRAINT [FK_property_brand]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_property_department]') AND parent_object_id = OBJECT_ID(N'[dbo].[property]'))
ALTER TABLE [dbo].[property]  WITH CHECK ADD  CONSTRAINT [FK_property_department] FOREIGN KEY([department_id])
REFERENCES [dbo].[department] ([department_id])
GO
ALTER TABLE [dbo].[property] CHECK CONSTRAINT [FK_property_department]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_property_local]') AND parent_object_id = OBJECT_ID(N'[dbo].[property]'))
ALTER TABLE [dbo].[property]  WITH CHECK ADD  CONSTRAINT [FK_property_local] FOREIGN KEY([locale_id])
REFERENCES [dbo].[locale] ([locale_id])
GO
ALTER TABLE [dbo].[property] CHECK CONSTRAINT [FK_property_local]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_property_propertyclass]') AND parent_object_id = OBJECT_ID(N'[dbo].[property]'))
ALTER TABLE [dbo].[property]  WITH CHECK ADD  CONSTRAINT [FK_property_propertyclass] FOREIGN KEY([pc_id])
REFERENCES [dbo].[property_class] ([pc_id])
GO
ALTER TABLE [dbo].[property] CHECK CONSTRAINT [FK_property_propertyclass]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_property_supplier]') AND parent_object_id = OBJECT_ID(N'[dbo].[property]'))
ALTER TABLE [dbo].[property]  WITH CHECK ADD  CONSTRAINT [FK_property_supplier] FOREIGN KEY([supplier_id])
REFERENCES [dbo].[supplier] ([supplier_id])
GO
ALTER TABLE [dbo].[property] CHECK CONSTRAINT [FK_property_supplier]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_user_department]') AND parent_object_id = OBJECT_ID(N'[dbo].[user]'))
ALTER TABLE [dbo].[user]  WITH CHECK ADD  CONSTRAINT [FK_user_department] FOREIGN KEY([department_id])
REFERENCES [dbo].[department] ([department_id])
GO
ALTER TABLE [dbo].[user] CHECK CONSTRAINT [FK_user_department]
