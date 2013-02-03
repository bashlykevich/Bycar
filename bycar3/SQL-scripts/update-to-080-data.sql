UPDATE [dbo].[Spare]
SET [dbo].[Spare].[QRest] = (SELECT [dbo].[GetQRests] ([id] , (SELECT s.[QRest] 
																FROM [dbo].[SpareView] as s 
																WHERE s.[ID] = [dbo].[Spare].[id])))

UPDATE [drive].[dbo].[spare]
SET [QRest] = '<r><w q="0" /><w q="0" /><w q="0" /></r>'
WHERE [QRest] IS NULL
