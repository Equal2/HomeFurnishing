USE [RUG]
GO

/****** Object:  UserDefinedFunction [Web].[FGetListofJobReceiveBarCode]    Script Date: 02/Sep/2015 4:47:59 PM ******/
IF object_id ('[Web].[FGetListofJobReceiveBarCode]') IS NOT NULL 
 DROP FUNCTION [Web].[FGetListofJobReceiveBarCode]
GO

/****** Object:  UserDefinedFunction [Web].[FGetListofJobReceiveBarCode]    Script Date: 02/Sep/2015 4:47:59 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

Create FUNCTION [Web].[FGetListofJobReceiveBarCode] (@JobReceiveHeaderId INTEGER, @JobOrderLineId INTEGER  )
Returns NVARCHAR (Max) AS
BEGIN 

DECLARE @FGetListofBaleNo NVARCHAR (Max) 

declare @TempLabelTable TABLE (ID int identity, BaleNo NVARCHAR (20))

insert into @TempLabelTable(BaleNo)
SELECT PU.ProductUidName AS BaleNo
FROM Web._JobReceiveLines L
LEFT JOIN web.ProductUids PU ON PU.ProductUIDId = L.ProductUidId 
WHERE L.CreatedDate  IS NOT NULL  
AND L.JobReceiveHeaderId = @JobReceiveHeaderId
AND L.JobOrderLineId  = @JobOrderLineId
GROUP BY PU.ProductUidName 
ORDER BY (CASE WHEN isNumeric(CASE WHEN charindex('-',PU.ProductUidName) > 0 THEN LEFT (PU.ProductUidName, charindex('-',PU.ProductUidName)-1) ELSE PU.ProductUidName END ) > 0
THEN convert(NUMERIC,CASE WHEN charindex('-',PU.ProductUidName) > 0 THEN LEFT (PU.ProductUidName, charindex('-',PU.ProductUidName)-1) ELSE PU.ProductUidName END ) ELSE 0 END )




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


