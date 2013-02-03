GO
SET ANSI_NULLS, ANSI_PADDING, ANSI_WARNINGS, ARITHABORT, CONCAT_NULL_YIELDS_NULL, QUOTED_IDENTIFIER ON;

SET NUMERIC_ROUNDABORT OFF;


PRINT N'Altering [dbo].[spare]...';


GO
ALTER TABLE [dbo].[spare]
    ADD [QRest] XML NULL;




GO
CREATE FUNCTION [dbo].[GetQ]
(
	-- Add the parameters for the function here
	@SpareID int,
	@WarehouseID int
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @Q int;

	SET @Q =	COALESCE ((SELECT SUM(sisi.QRest)
				FROM dbo.SpareInSpareIncomeView as sisi
				WHERE sisi.[SpareID] = @SpareID AND sisi.[WarehouseID] = @WarehouseID),0);
	
	-- Return the result of the function
	RETURN @Q;
END
GO
PRINT N'Creating [dbo].[GetQRests]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date, ,>
-- Description:	<Description, ,>
-- =============================================
CREATE FUNCTION [dbo].[GetQRests]
(
	@SpareID int,
	@TotalQ float
)
RETURNS xml
AS
BEGIN
	-- Declare the return variable here
	DECLARE @rests xml;
	SET @rests = '';
	
	DECLARE @warehouses table (Q int NULL);
	
	INSERT @warehouses 
	SELECT [dbo].GetQ(@SpareID, W.[ID]) FROM dbo.warehouse as W;		
	
	DECLARE @sres varchar(100);
	SELECT @sres = COALESCE(@sres, '') + '<w q="' + CAST(Q AS varchar(5)) + '"/>'
	FROM @warehouses;
	
	SET @rests = '<r>' 
					+ '<w q="' + CAST(@TotalQ AS varchar(5)) + '"/>'
					+ @sres 
					+ '</r>';	
	
	-- Return the result of the function
	RETURN @rests;

END
GO
PRINT N'Creating [dbo].[SelectSpares]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE FUNCTION [dbo].[SelectSpares]
(@SpareID int = null)
RETURNS 
@SparesCash TABLE 
(
	   name varchar(750) NOT NULL
      ,q_rest decimal(10,0) NULL
      ,q_demand decimal(8,0) NULL
      ,code varchar(250) NOT NULL
      ,codeShatem varchar(250) NOT NULL
      ,is_equal bit NULL
      ,BrandName varchar(50) NOT NULL
      ,QRest float NULL
      ,spare_group1_id int NULL
      ,spare_group3_id int NULL
      ,spare_group2_id int NULL
      ,GroupID int NOT NULL
      ,demand decimal(8,0) NULL
      ,BrandID int NOT NULL
      ,UnitID int NOT NULL
      ,id int NOT NULL	
      ,QRests xml NULL
)
AS
BEGIN
	
	INSERT @SparesCash
    SELECT [name]
      ,[q_rest]
      ,[q_demand]
      ,[code]
      ,[codeShatem]
      ,[is_equal]
      ,[BrandName]
      ,[QRest]
      ,[spare_group1_id]
      ,[spare_group3_id]
      ,[spare_group2_id]
      ,[GroupID]
      ,[demand]
      ,[BrandID]
      ,[UnitID]
      ,[id], [dbo].GetQRests([id],[QRest])     
    FROM [dbo].[SpareView] as SV
    WHERE (SV.[id] = @SpareID OR @SpareID IS NULL)
	RETURN 
END

GO
PRINT N'Creating [dbo].[sp_Spares]...';


GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[sp_Spares] 	
(
@SpareID int = null
)
AS SELECT * FROM [dbo].[SelectSpares] (@SpareID)
GO
PRINT N'Creating [dbo].[GetQ]...';


GO
PRINT N'Refreshing [dbo].[SpareInOverpricingView]...';


GO
EXECUTE sp_refreshview N'dbo.SpareInOverpricingView';


GO
PRINT N'Refreshing [dbo].[SpareInSpareIncomeView]...';


GO
EXECUTE sp_refreshview N'dbo.SpareInSpareIncomeView';


GO
PRINT N'Refreshing [dbo].[ReportIncomes]...';


GO
EXECUTE sp_refreshview N'dbo.ReportIncomes';


GO
PRINT N'Altering [dbo].[SpareView]...';


GO
ALTER VIEW dbo.SpareView
AS
SELECT     TOP (100) PERCENT S.name, S.q_rest, S.q_demand, S.code, S.codeShatem, S.is_equal, B.name AS BrandName, CAST
                          ((SELECT     COALESCE (SUM(CAST(QRest AS float)), 0) AS Expr1
                              FROM         dbo.spare_in_spare_income AS sisi
                              WHERE     (QRest > 0) AND (SpareID = S.id)) AS float) AS QRest, S.spare_group1_id, S.spare_group3_id, S.spare_group2_id, S.spare_group_id AS GroupID, 
                      S.q_demand_clear AS demand, S.brand_id AS BrandID, S.unit_id AS UnitID, S.id, S.QRest AS QRests
FROM         dbo.spare AS S LEFT OUTER JOIN
                      dbo.brand AS B ON B.id = S.brand_id
ORDER BY S.code
GO
PRINT N'Refreshing [dbo].[ReportOutgoes]...';


GO
EXECUTE sp_refreshview N'dbo.ReportOutgoes';


GO
PRINT N'Refreshing [dbo].[SpareInSpareOutgoView]...';


GO
EXECUTE sp_refreshview N'dbo.SpareInSpareOutgoView';


GO
PRINT N'Refreshing [dbo].[SpareInInvoiceView]...';


GO
EXECUTE sp_refreshview N'dbo.SpareInInvoiceView';


GO
PRINT N'Refreshing [dbo].[BasketView]...';


GO
EXECUTE sp_refreshview N'dbo.BasketView';


GO
