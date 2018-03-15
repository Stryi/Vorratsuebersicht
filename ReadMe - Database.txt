--------------------------------------------------------------------------------
Expiration date monitoring   (Vorraete)
--------------------------------------------------------------------------------
	
Datenbankdefinition
--------------------------------------------------------------------------------

=======
Article
=======

Schlüssel: EAN-Code
Schlüssel: Name, Manufacturer, Size, Unit (wenn EAN-Code leer)


ArticleId    EAN-Code  Name                               Manufacturer   Category      DurableInfinity   WarnInDays   Size      Unit     Image
------------ --------- ---------------------------------- -------------- ------------- ----------------- ------------ --------- -------- ---------------------------
000000000001;20725488  Ravioli in Tomatensauce            Combino        Essen         Nein              30           800       g        0x5435354354...
000000000002;20622374  Pfirsichhälften leicht gezuckert   Freshona       Trinken       Nein              30           820       g        0x526355757F...
000000000003;          Wasser                             Silberbrunnen  Trinken       Nein              20           0.75      l        0x9567898778...
000000000004           Aspirin                            Ratiopharm     Medikament    Nein              120          15                 0x3565436465...
000000000005           Lampenöl                                          Brennstoff    Ja                             2         l        0x7846784787...

=======
Storage
=======

Schlüssel: Name

StorageId    Name                 Image
------------ -------------------- -------------------------
000000000301 Keller               0x123837AC4F...
000000000302 Dachgeschoss         0x7382871762...

===========
StorageItem
===========

Schlüssel: StorageId, ArticleId, BestBefore 

StorageItemId StorageId    ArticleId    BestBefore   Quantity
------------- ------------ ------------ ------------ --------
000000000101  000000000301 000000000001 31.10.2019         20
000000000102  000000000302 000000000002 31.12.2018          2


Wenn Schlüssel doppelt -> Aufsummieren und löschen

==================
StorageTransaction
==================

StorageTransactionId   StorageId    ArticleId     Quantity  Reason     PricePerUnit  BoughtOn  BoughtIn
---------------------- ------------ ------------- --------- ---------- ------------- --------- ----------------
000000000201           000000000301 000000000001          2 Einkauf             0.99 06.07.16 LIDL Winnenden   
000000000202           000000000302 000000000002          2 Entnahme            
000000000203           000000000301 000000000002         -2 Umlagerung          
000000000204           000000000302 000000000002          2 Umlagerung          
000000000205           000000000302 000000000002          1 Korrektur           
000000000206           000000000302 000000000002         -1 Verfallen,Kaputt,Beschädigt



--------------------------------------------------------------------------------
Artikel, die abgelaufen sind
--------------------------------------------------------------------------------

SELECT SUM(Quantity) AS Quantity
	FROM StorageItem
	WHERE BestBefore < date('now')


--------------------------------------------------------------------------------
Artikel, die kurz vorm Ablaufen sind, aber noch nicht abgelaufen sind.
--------------------------------------------------------------------------------

SELECT SUM(Quantity) AS Quantity
	FROM StorageItem
	JOIN Article ON StorageItem.ArticleId = Article.ArticleId
	WHERE date(StorageItem.BestBefore,  (-Article.WarnInDays || ' day')) <= date('now')
	AND StorageItem.BestBefore >= date('now')

--------------------------------------------------------------------------------
Alle existierenden Kategorien
--------------------------------------------------------------------------------

SELECT DISTINCT Category AS Value
 FROM Article
 WHERE Category IS NOT NULL 
 AND ArticleId IN (SELECT ArticleId FROM StorageItem)
 ORDER BY Category

--------------------------------------------------------------------------------
Alle existierenden Unterkategorien
--------------------------------------------------------------------------------

SELECT DISTINCT SubCategory AS Value
 FROM Article
 WHERE Category = 'Essen'
 AND SubCategory IS NOT NULL 
 AND ArticleId IN (SELECT ArticleId FROM StorageItem)
 ORDER BY SubCategory

