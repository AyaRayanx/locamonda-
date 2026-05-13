using Microsoft.AspNetCore.Identity;
using locamonda.Models;
using locamonda.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class DataSeeder {
    private readonly AppDbContext _context;
    private readonly UserManager<Users> _userManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;

    public DataSeeder(AppDbContext context, UserManager<Users> userManager, RoleManager<IdentityRole<int>> roleManager) {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task SeedAll() {
        Console.WriteLine("Seeding Roles...");
        string[] roles = { "Admin", "Owner", "Customer" };
        foreach (var role in roles) {
            if (!await _roleManager.RoleExistsAsync(role)) {
                await _roleManager.CreateAsync(new IdentityRole<int>(role));
            }
        }

        Console.WriteLine("Seeding Categories...");
        if (!_context.Categories.Any()) {
            _context.Categories.AddRange(new List<Category> {
                new Category { Name = "Luxury Hotel", Description = "Five-star premium experience" },
                new Category { Name = "Apartment", Description = "Modern city apartments" },
                new Category { Name = "Villa", Description = "Private luxury villas" },
                new Category { Name = "Studio", Description = "Compact living spaces" },
                new Category { Name = "Duplex", Description = "Two-floor residences" },
                new Category { Name = "Penthouse", Description = "Top-floor luxury suites" },
                new Category { Name = "Beach House", Description = "Waterfront properties" },
                new Category { Name = "Chalet", Description = "Mountain or seaside retreats" }
            });
            await _context.SaveChangesAsync();
        }

        Console.WriteLine("Seeding Locations...");
        if (!_context.Locations.Any()) {
            _context.Locations.AddRange(new List<Location> {
                new Location { Country = "Egypt", City = "Cairo", District = "Zamalek", ZipCode = "11211" },
                new Location { Country = "Egypt", City = "Alexandria", District = "Stanley", ZipCode = "21500" },
                new Location { Country = "Egypt", City = "Giza", District = "Pyramids", ZipCode = "12511" },
                new Location { Country = "Egypt", City = "Hurghada", District = "Marina", ZipCode = "84511" },
                new Location { Country = "Egypt", City = "Sharm El-Sheikh", District = "Naama Bay", ZipCode = "46619" }
            });
            await _context.SaveChangesAsync();
        }

        Console.WriteLine("Seeding Users...");
        var admin = await CreateUser("admin@locamonda.com", "Admin User", "Admin123!", "Admin");
        var owner1 = await CreateUser("owner1@locamonda.com", "John Owner", "Owner123!", "Owner");
        var owner2 = await CreateUser("owner2@locamonda.com", "Sarah Host", "Owner123!", "Owner");
        var user1 = await CreateUser("user1@example.com", "Alice Guest", "User123!", "Customer");

        Console.WriteLine("Seeding Properties...");
        var cat1 = _context.Categories.FirstOrDefault(c => c.Name == "Luxury Hotel") ?? _context.Categories.First();
        var loc1 = _context.Locations.FirstOrDefault(l => l.City == "Cairo") ?? _context.Locations.First();
        var loc2 = _context.Locations.FirstOrDefault(l => l.City == "Alexandria") ?? _context.Locations.Skip(1).First();

        var nileView = "Nile View Premium Hotel";
        if (!_context.Properties.Any(p => p.Title == nileView)) {
            var prop1 = new Property {
                Title = nileView,
                Description = "Stunning views of the Nile River in the heart of Cairo. Experience world-class hospitality.",
                Price = 2500.00m,
                Rooms = 4,
                Bathrooms = 3,
                Status = "Available",
                DateAdded = DateTime.Now,
                IsActive = true,
                IsApproved = true,
                OwnerId = owner1.Id,
                LocationId = loc1.LocationId,
                CategoryId = cat1.CategoryId,
                Latitude = 30.0444,
                Longitude = 31.2357,
                Area = 120,
                HasWifi = true,
                HasAC = true,
                HasGym = true
            };
            _context.Properties.Add(prop1);
            await _context.SaveChangesAsync();
            _context.Photos.Add(new Photo { PhotoUrl = "/images/hotel1.jpg", IsMain = true, PropertyId = prop1.PropertyId, UploadedAt = DateTime.Now });
        }

        var alexMed = "Alexandria Mediterranean Resort";
        if (!_context.Properties.Any(p => p.Title == alexMed)) {
            var prop2 = new Property {
                Title = alexMed,
                Description = "Luxury resort overlooking the Mediterranean Sea. Perfect for summer vacations.",
                Price = 1850.00m,
                Rooms = 2,
                Bathrooms = 2,
                Status = "Available",
                DateAdded = DateTime.Now,
                IsActive = true,
                IsApproved = false,
                OwnerId = owner2.Id,
                LocationId = loc2.LocationId,
                CategoryId = cat1.CategoryId,
                Latitude = 31.2001,
                Longitude = 29.9187,
                Area = 85,
                HasWifi = true,
                HasAC = true,
                HasGarden = true
            };
            _context.Properties.Add(prop2);
            await _context.SaveChangesAsync();
            _context.Photos.Add(new Photo { PhotoUrl = "/images/hotel2.jpeg", IsMain = true, PropertyId = prop2.PropertyId, UploadedAt = DateTime.Now });
        }

        var pyramidStay = "Pyramid View Boutique";
        if (!_context.Properties.Any(p => p.Title == pyramidStay)) {
            var loc3 = _context.Locations.FirstOrDefault(l => l.City == "Giza") ?? _context.Locations.First();
            var prop3 = new Property {
                Title = pyramidStay,
                Description = "Wake up to the view of the Great Pyramids. Authentic Egyptian experience with modern comfort.",
                Price = 1950.00m,
                Rooms = 3,
                Bathrooms = 2,
                Status = "Available",
                DateAdded = DateTime.Now,
                IsActive = true,
                IsApproved = true,
                OwnerId = owner1.Id,
                LocationId = loc3.LocationId,
                CategoryId = cat1.CategoryId,
                Latitude = 29.9792,
                Longitude = 31.1342,
                Area = 150,
                HasWifi = true,
                HasAC = true,
                HasHeating = true
            };
            _context.Properties.Add(prop3);
            await _context.SaveChangesAsync();
            _context.Photos.Add(new Photo { PhotoUrl = "/images/hotel3.webp", IsMain = true, PropertyId = prop3.PropertyId, UploadedAt = DateTime.Now });
        }
        await _context.SaveChangesAsync();

        Console.WriteLine("Seeding Reports...");
        if (!_context.Reports.Any()) {
            var reporter = await _userManager.FindByEmailAsync("user1@example.com");
            var reported = await _userManager.FindByEmailAsync("owner1@locamonda.com");
            var property = _context.Properties.First();

            _context.Reports.Add(new Report {
                ReporterId = reporter.Id,
                ReportedUserId = reported.Id,
                PropertyId = property.PropertyId,
                Reason = "The property description is misleading regarding the distance to the beach.",
                Status = "Pending",
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
        }
    }

    private async Task<Users> CreateUser(string email, string name, string password, string role) {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) {
            user = new Users {
                UserName = email,
                Email = email,
                Name = name,
                AccountType = role,
                IsActive = true,
                Address = "Test Address",
                Age = 30,
                CreatedAt = DateTime.Now,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded) {
                await _userManager.AddToRoleAsync(user, role);
            }
        }
        return user;
    }
}
