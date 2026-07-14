using ASProducts.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ASProducts.Api.Data.Seeds;

public static class SeedData
{
    public static async Task Initialize(ProductsContext context)
    {
        // --------------------------------- Reset DBContext -----------------------------------
        // context.Products.RemoveRange(await context.Products.ToListAsync());
        // await context.SaveChangesAsync();
        // await context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('Products', RESEED, 0)");
        // --------------------------------- Reset DBContext -----------------------------------

        if (await context.Products.AnyAsync()) return;

        string sourcePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seeds", "Images");
        string targetPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");

        if (!Directory.Exists(targetPath)) Directory.CreateDirectory(targetPath);

        if (Directory.Exists(sourcePath))
        {
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*"))
            {
                string fileName = Path.GetFileName(newPath);
                File.Copy(newPath, Path.Combine(targetPath, fileName), true);
            }
        }

        context.Products.AddRange(
            new Product { Name = "Auto a control remoto", Company = "ToyCo", Price = 45.00m, RestrictionAge = 6, Description = "Auto deportivo de alta velocidad con luces LED." },
            new Product { Name = "Muñeca de colección", Company = "Mattel", Price = 25.50m, RestrictionAge = 3 },
            new Product { Name = "Bloques de construcción", Company = "BrickMaster", Price = 89.99m, RestrictionAge = 8, Description = "Set de 500 piezas para construir una ciudad." },
            new Product { Name = "Peluche de oso", Company = "Softies", Price = 15.00m },
            new Product { Name = "Rompecabezas 3D", Company = "PuzzleFun", Price = 22.00m, RestrictionAge = 10, Description = "Réplica del Taj Mahal en 3D." },
            new Product { Name = "Dron para principiantes", Company = "SkyHigh", Price = 120.50m, RestrictionAge = 12 },
            new Product { Name = "Kit de química", Company = "ScienceLab", Price = 35.00m, RestrictionAge = 10, Description = "Incluye 20 experimentos seguros para niños." },
            new Product { Name = "Pelota de fútbol", Company = "SportPro", Price = 12.99m },
            new Product { Name = "Juego de mesa Estrategia", Company = "BoardWorld", Price = 55.00m, RestrictionAge = 14, Description = "Juego de conquista territorial de larga duración." },
            new Product { Name = "Caballete de arte", Company = "CreativeMind", Price = 95.00m, RestrictionAge = 5 }
        );

        await context.SaveChangesAsync();
    }
}