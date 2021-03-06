DISABLE TRIGGER [dbo].[OfferingIncomeOnInsert] ON [Drive-068-LE].[dbo].[spare_in_spare_income]
GO
DISABLE TRIGGER [dbo].[OfferingOutgoOnInsert] ON [Drive-068-LE].[dbo].[spare_in_spare_outgo]
GO

DELETE
  FROM [Drive-068-LE].[dbo].[currency_rate]
  WHERE rate = 0

UPDATE [Drive-068-LE].[dbo].Basket
SET Pbyr = Pbyr / 10000;
GO

UPDATE [Drive-068-LE].[dbo].[currency_rate]
SET rate = rate / 10000;
GO

UPDATE [Drive-068-LE].[dbo].[invoice]
SET [InvoiceSum] = [InvoiceSum]/10000;
GO

UPDATE [Drive-068-LE].[dbo].[Sale]
SET Pbyr = Pbyr/10000;
GO

UPDATE [Drive-068-LE].[dbo].spare_in_invoice
SET price = price/10000,
	vat_rate_sum = vat_rate_sum/10000,
	total_sum = total_sum/10000,
	total_with_vat = total_with_vat/10000;
GO

UPDATE [Drive-068-LE].[dbo].spare_in_overpricing
SET [purchasePrice] = [purchasePrice]/10000
      ,[percentOld] = [percentOld]/10000
      ,[priceOld] = [priceOld]/10000
      ,[sumOld] = [sumOld]/10000
      ,[priceNew] = [priceNew]/10000
      ,[sumNew] = [sumNew]/10000
GO

UPDATE [Drive-068-LE].[dbo].spare_in_spare_income
SET [PIn] = [PIn]/10000
      ,[PInBasic] = [PInBasic]/10000
      ,[S] = [S]/10000
      ,[SBasic] = [SBasic]/10000
      ,[POut] = [POut]/10000
      ,[POutBasic] = [POutBasic]/10000
WHERE CurrencyID = 1
GO

UPDATE [Drive-068-LE].[dbo].spare_in_spare_income
SET [PInBasic] = [PInBasic]/10000
      ,[SBasic] = [SBasic]/10000
      ,[POutBasic] = [POutBasic]/10000
WHERE CurrencyID != 1
GO

UPDATE [Drive-068-LE].[dbo].[spare_in_spare_outgo]
SET [purchase_price] = [purchase_price]/10000
      ,[total_sum] = [total_sum]/10000
      ,[basic_price] = [basic_price]/10000
      ,[discount] = [discount]/10000
GO


ENABLE TRIGGER [dbo].[OfferingIncomeOnInsert] ON [Drive-068-LE].[dbo].[spare_in_spare_income]
GO
ENABLE TRIGGER [dbo].[OfferingOutgoOnInsert] ON [Drive-068-LE].[dbo].[spare_in_spare_outgo]
GO
 