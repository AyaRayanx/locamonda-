USE locamonda;
GO

DECLARE @OwnerId INT;

SELECT @OwnerId = Id FROM AspNetUsers WHERE Email = 'amirowner@gmail.com';

IF @OwnerId IS NULL
BEGIN
    PRINT 'User amirowner@gmail.com not found.';
    RETURN;
END

-- Delete previously inserted properties by this owner to avoid duplicates
DELETE FROM Photos WHERE PropertyId IN (SELECT PropertyId FROM Properties WHERE OwnerId = @OwnerId);
DELETE FROM Properties WHERE OwnerId = @OwnerId;

-- Categories
DECLARE @CatApt INT, @CatVilla INT, @CatChalet INT, @CatOffice INT, @CatStudio INT;
IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Apartment') INSERT INTO Categories (Name) VALUES (N'Apartment');
SELECT @CatApt = CategoryId FROM Categories WHERE Name = N'Apartment';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Villa') INSERT INTO Categories (Name) VALUES (N'Villa');
SELECT @CatVilla = CategoryId FROM Categories WHERE Name = N'Villa';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Chalet') INSERT INTO Categories (Name) VALUES (N'Chalet');
SELECT @CatChalet = CategoryId FROM Categories WHERE Name = N'Chalet';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Office') INSERT INTO Categories (Name) VALUES (N'Office');
SELECT @CatOffice = CategoryId FROM Categories WHERE Name = N'Office';

IF NOT EXISTS (SELECT 1 FROM Categories WHERE Name = N'Studio') INSERT INTO Categories (Name) VALUES (N'Studio');
SELECT @CatStudio = CategoryId FROM Categories WHERE Name = N'Studio';

-- Locations
DECLARE @LocCairo INT, @LocAlex INT, @LocGiza INT, @LocHurghada INT, @LocDubai INT;

IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = N'Cairo') INSERT INTO Locations (Country, City) VALUES (N'Egypt', N'Cairo');
SELECT @LocCairo = LocationId FROM Locations WHERE City = N'Cairo';

IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = N'Alexandria') INSERT INTO Locations (Country, City) VALUES (N'Egypt', N'Alexandria');
SELECT @LocAlex = LocationId FROM Locations WHERE City = N'Alexandria';

IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = N'Giza') INSERT INTO Locations (Country, City) VALUES (N'Egypt', N'Giza');
SELECT @LocGiza = LocationId FROM Locations WHERE City = N'Giza';

IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = N'Hurghada') INSERT INTO Locations (Country, City) VALUES (N'Egypt', N'Hurghada');
SELECT @LocHurghada = LocationId FROM Locations WHERE City = N'Hurghada';

IF NOT EXISTS (SELECT 1 FROM Locations WHERE City = N'Dubai') INSERT INTO Locations (Country, City) VALUES (N'UAE', N'Dubai');
SELECT @LocDubai = LocationId FROM Locations WHERE City = N'Dubai';

DECLARE @PropId INT;

-- Property 1
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Luxury Apartment in New Cairo', N'Stunning super lux apartment with a spacious layout in the best areas of New Cairo. Features all amenities and garden views.', 3500000, 3, 2, 180, 'Available', GETDATE(), 1, 1, 1, 1, 0, 1, 0, @OwnerId, @LocCairo, @CatApt);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1522708323590-d24dbb6b0267?w=800', 1, GETDATE(), @PropId);

-- Property 2
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Villa with Garden and Pool', N'Highly elegant standalone villa in Sheikh Zayed, featuring a large garden and a private swimming pool. Ultra super lux finish with a modern design.', 15000000, 5, 4, 450, 'Available', GETDATE(), 1, 1, 1, 1, 1, 1, 1, @OwnerId, @LocGiza, @CatVilla);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1600596542815-ffad4c1539a9?w=800', 1, GETDATE(), @PropId);

-- Property 3
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Fully Equipped Office Space', N'Office for rent or sale in Downtown Cairo, fully equipped with all necessary connections and fully air-conditioned, suitable for a company or clinic.', 2000000, 4, 1, 120, 'Available', GETDATE(), 1, 1, 1, 1, 0, 0, 0, @OwnerId, @LocCairo, @CatOffice);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1497366216548-37526070297c?w=800', 1, GETDATE(), @PropId);

-- Property 4
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Seafront Summer Apartment', N'Excellent apartment in Sidi Bishr, Alexandria directly overlooking the sea. Suitable for summer vacations or great tourism investment.', 2500000, 2, 1, 100, 'Available', GETDATE(), 1, 1, 1, 0, 0, 0, 0, @OwnerId, @LocAlex, @CatApt);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=800', 1, GETDATE(), @PropId);

-- Property 5
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Classic Villa in Maadi', N'Villa with a charming classic style in the quiet Maadi area, containing old trees, vast green spaces, and high-end wooden finishes.', 12000000, 6, 5, 500, 'Available', GETDATE(), 1, 1, 1, 1, 1, 0, 1, @OwnerId, @LocCairo, @CatVilla);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1600585154340-be6161a56a0c?w=800', 1, GETDATE(), @PropId);

-- Property 6
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Furnished Chalet in Hurghada', N'Sea view chalet in an integrated tourist village in Hurghada, featuring hotel furniture, fully air-conditioned and ready to move in.', 4200000, 2, 2, 110, 'Available', GETDATE(), 1, 1, 1, 1, 0, 1, 1, @OwnerId, @LocHurghada, @CatChalet);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1499793983690-e29da59ef1c2?w=800', 1, GETDATE(), @PropId);

-- Property 7
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Luxury Studio for Rent in Dubai Marina', N'Fully furnished studio with very high-end modern furniture overlooking the Marina. The building has a pool, gym, and 24/7 security.', 1800000, 1, 1, 60, 'Available', GETDATE(), 1, 1, 1, 1, 0, 1, 0, @OwnerId, @LocDubai, @CatStudio);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1536376072261-38c75010e6c9?w=800', 1, GETDATE(), @PropId);

-- Property 8
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Duplex Apartment in Mohandeseen', N'Huge duplex in the heart of Mohandeseen, strategic location and quiet street, imported marble finish and modern lighting.', 7500000, 4, 3, 300, 'Available', GETDATE(), 1, 1, 1, 1, 0, 0, 0, @OwnerId, @LocGiza, @CatApt);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800', 1, GETDATE(), @PropId);

-- Property 9
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Corporate Headquarters Office', N'Full administrative floor in Smart Village suitable for major companies, equipped with high-speed internet, central AC, and glass offices.', 25000000, 10, 4, 800, 'Available', GETDATE(), 1, 1, 1, 1, 1, 0, 0, @OwnerId, @LocGiza, @CatOffice);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1504384308090-c894fdcc538d?w=800', 1, GETDATE(), @PropId);

-- Property 10
INSERT INTO Properties (Title, Description, Price, Rooms, Bathrooms, Area, Status, DateAdded, IsActive, IsApproved, HasWifi, HasAC, HasHeating, HasGym, HasGarden, OwnerId, LocationId, CategoryId)
VALUES (N'Panoramic Nile View Apartment', N'Charming apartment in Zamalek with a clear panoramic view of the Nile River, luxury finishes, central AC, and private parking.', 14000000, 3, 3, 220, 'Available', GETDATE(), 1, 1, 1, 1, 0, 0, 0, @OwnerId, @LocCairo, @CatApt);
SET @PropId = SCOPE_IDENTITY();
INSERT INTO Photos (PhotoUrl, IsMain, UploadedAt, PropertyId) VALUES ('https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=800', 1, GETDATE(), @PropId);

PRINT N'Successfully deleted old properties and inserted 10 English properties with Photos!';
