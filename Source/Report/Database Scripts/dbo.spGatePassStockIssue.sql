
IF OBJECT_ID ('Web.spGatePassStockIssue') IS NOT NULL
	DROP PROCEDURE Web.spGatePassStockIssue	
GO
/****** Object:  StoredProcedure [dbo].[CreateNotification]    Script Date: 04/Aug/2015 11:59:41 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE Web.spGatePassStockIssue
@Id INT 
AS 
Begin

SELECT P.ProductName, L.Specification, sum(L.Qty) AS Qty, Max(P.UnitId) AS UnitId     
FROM web.StockHeaders H WITH (Nolock)
LEFT JOIN web.StockLines L WITH (Nolock) ON L.StockHeaderId = H.StockHeaderId 
LEFT JOIN WEB.Products P WITH (Nolock) ON L.ProductId = P.ProductId
WHERE H.StockHeaderId = @Id
GROUP BY P.ProductName, L.Specification
End
GO



