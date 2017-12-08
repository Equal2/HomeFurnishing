IF object_id ('[Web].[FGetListofPackingBaleNo]') IS NOT NULL 
DROP FUNCTION [Web].[FGetListofPackingBaleNo]
GO 

CREATE FUNCTION [Web].FGetListofPackingBaleNo (@PackingHeaderId INTEGER, @ProductInvoiceGroupNameId INTEGER, @SaleOrderHeaderId INTEGER = NULL  )
Returns NVARCHAR (Max) AS
BEGIN 

DECLARE @FGetListofBaleNo NVARCHAR (Max) 

declare @TempLabelTable TABLE (ID int identity, BaleNo NVARCHAR (20))

insert into @TempLabelTable(BaleNo)
SELECT L.BaleNo
FROM Web.PackingLines L WITH (Nolock)
LEFT JOIN web.PackingHeaders H WITH (Nolock) ON H.PackingHeaderId = L.PackingHeaderId 
LEFT JOIN web.Divisions D WITH (Nolock) ON D.DivisionId = H.DivisionId 
LEFT JOIN web.People B WITH (Nolock) ON B.PersonID = H.BuyerId  
LEFT JOIN web.People JW WITH (Nolock) ON JW.PersonID = H.JobWorkerId 
LEFT JOIN Web.Products P WITH (Nolock) ON P.ProductId = L.ProductId 
LEFT JOIN Web.FinishedProduct  FPD WITH (Nolock) ON FPD.ProductId = L.ProductId 
LEFT JOIN Web.ProductInvoiceGroups  PIG WITH (Nolock) ON PIG.ProductInvoiceGroupId = FPD.ProductInvoiceGroupId 
LEFT JOIN web.SaleOrderLines SOL ON SOL.SaleOrderLineId = L.SaleOrderLineId 
LEFT JOIN Web.ViewRugSize UC WITH (Nolock) ON UC.ProductId = L.ProductId  
WHERE L.CreatedDate  IS NOT NULL  
--AND H.Status > 1 AND H.Status < 5
--AND H.PackingHeaderId = 3051
--AND PIG.ProductInvoiceGroupName = 'HAND TUFTED WOOLLEN CARPET(FINE)'
AND H.PackingHeaderId = @PackingHeaderId
AND isnull(FPD.ProductInvoiceGroupId,0)  = isnull(@ProductInvoiceGroupNameId,0)

AND (@SaleOrderHeaderId is null OR SOL.SaleOrderHeaderId IN (SELECT Items FROM [dbo].[Split] (@SaleOrderHeaderId, ',')))
--AND isnull(SOL.SaleOrderHeaderId,0)  = isnull(@SaleOrderHeaderId,0)
GROUP BY L.BaleNo 
ORDER BY (CASE WHEN isNumeric(CASE WHEN charindex('-',L.BaleNo) > 0 THEN LEFT (L.BaleNo, charindex('-',L.BaleNo)-1) ELSE L.BaleNo END ) > 0
THEN convert(NUMERIC,CASE WHEN charindex('-',L.BaleNo) > 0 THEN LEFT (L.BaleNo, charindex('-',L.BaleNo)-1) ELSE L.BaleNo END ) ELSE 0 END )


DECLARE @RowsCount INT;
SET @RowsCount = ( SELECT count(*) FROM @TempLabelTable)

        
DECLARE @I int
DECLARE @MRoll NVARCHAR (Max) 
DECLARE @Troll int 
DECLARE @froll FLOAT  
DECLARE @subRollCount int 
declare @BaleNo varchar(20)
declare @BaleNoBefore varchar(20)

SET  @I = 0
SET  @froll = 0
SET  @subRollCount = 0
SET @BaleNoBefore =0

declare cur2 cursor for select 
(CASE WHEN isNumeric(CASE WHEN charindex('-',L.BaleNo) > 0 THEN LEFT (L.BaleNo, charindex('-',L.BaleNo)-1) ELSE L.BaleNo END ) > 0
THEN convert(NUMERIC,CASE WHEN charindex('-',L.BaleNo) > 0 THEN LEFT (L.BaleNo, charindex('-',L.BaleNo)-1) ELSE L.BaleNo END ) ELSE 0 END ) AS BaleNo 
from @TempLabelTable L 
ORDER BY (CASE WHEN isNumeric(CASE WHEN charindex('-',L.BaleNo) > 0 THEN LEFT (L.BaleNo, charindex('-',L.BaleNo)-1) ELSE L.BaleNo END ) > 0
THEN convert(NUMERIC,CASE WHEN charindex('-',L.BaleNo) > 0 THEN LEFT (L.BaleNo, charindex('-',L.BaleNo)-1) ELSE L.BaleNo END ) ELSE 0 END )

open cur2

fetch next from cur2 into @BaleNo

WHILE   @@FETCH_STATUS = 0   
begin
SET @I = @I + 1


                    If ( @froll = 0 )                     
	                    BEGIN 
	                      SET  @froll = @BaleNo
	                      SET  @MRoll = @BaleNo
	                      SET @subRollCount = 0
	                    END                     
                    ELSE If ( @froll + 1 <> @BaleNo) 
	                    BEGIN
	                        If ( @subRollCount >= 1 )
	                           SET @MRoll = @MRoll + '-' + @BaleNoBefore + ', ' + @BaleNo
	                        Else
	                           SET @MRoll = @MRoll + ', ' + @BaleNo
	                           	                      
	                        SET @froll = @BaleNo
	                        SET @subRollCount = 0
	                     END 
	                 ELSE
		                 begin
	                       SET @froll = @BaleNo
	                       SET @subRollCount = @subRollCount + 1
	                 	  End 
	                 
	               If ( @I = @RowsCount  )
	                BEGIN
	                IF (@subRollCount <> 0 )
	                BEGIN 
                        If ( @froll <> @BaleNo ) 
                           SET  @MRoll = @MRoll + ', ' + @BaleNo + ''
                        Else
                           SET @MRoll = @MRoll + '-' + @BaleNo + ''
                     End 
                    End 
                   SET  @BaleNoBefore = @BaleNo
                        
FETCH NEXT FROM cur2 into @BaleNo

end

close cur2

deallocate cur2
--print @MRoll 

SET @FGetListofBaleNo = @MRoll;
RETURN @FGetListofBaleNo;
END
GO


