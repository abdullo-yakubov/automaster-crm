using AutoMaster.Domain.Entities;
using AutoMaster.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AutoMaster.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db, bool seedDemoData = true)
    {
        await db.Database.MigrateAsync();

        await EnsureReferenceDataAsync(db);

        if (!seedDemoData || await db.Users.AnyAsync()) return;

        await SeedDataAsync(db);
    }

    /// <summary>Справочные данные, нужные системе в любом окружении.</summary>
    private static async Task EnsureReferenceDataAsync(AppDbContext db)
    {
        if (!await db.ServiceBoxes.AnyAsync())
        {
            var boxes = new[] { ("Бокс 1", "Двигатель"), ("Бокс 2", "Подвеска"), ("Бокс 3", "Электрика"),
                ("Бокс 4", "Кузов"), ("Бокс 5", "Шиномонтаж"), ("Бокс 6", "Универсальный") };
            db.ServiceBoxes.AddRange(boxes.Select((b, i) => new ServiceBox { Name = b.Item1, Specialization = b.Item2, SortOrder = i + 1 }));
            await db.SaveChangesAsync();
        }
    }

    public static async Task ResetAsync(AppDbContext db)
    {
        db.ServiceBoxes.RemoveRange(db.ServiceBoxes);
        db.MarketingCampaigns.RemoveRange(db.MarketingCampaigns);
        db.PromoCodes.RemoveRange(db.PromoCodes);
        db.Orders.RemoveRange(db.Orders);
        db.Vehicles.RemoveRange(db.Vehicles);
        db.Customers.RemoveRange(db.Customers);
        db.Mechanics.RemoveRange(db.Mechanics);
        db.Parts.RemoveRange(db.Parts);
        db.Transactions.RemoveRange(db.Transactions);
        db.Bookings.RemoveRange(db.Bookings);
        db.LoyaltyTransactions.RemoveRange(db.LoyaltyTransactions);
        db.Branches.RemoveRange(db.Branches);
        db.AppSettings.RemoveRange(db.AppSettings);
        db.ChecklistItems.RemoveRange(db.ChecklistItems);
        await db.SaveChangesAsync();

        await SeedDataAsync(db);
        await EnsureReferenceDataAsync(db);
    }

    private static async Task SeedDataAsync(AppDbContext db)
    {
        if (!await db.Users.AnyAsync())
        {
            db.Users.AddRange(
                new User { Email = "admin@automaster.tj", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!", 12), FullName = "Фарход Раҳимов", Role = UserRole.Admin, Phone = "+992900000001" },
                new User { Email = "manager@automaster.tj", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Manager123!", 12), FullName = "Дилноза Каримова", Role = UserRole.Manager, Phone = "+992900000002" },
                new User { Email = "mechanic@automaster.tj", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Mechanic123!", 12), FullName = "Шерали Назаров", Role = UserRole.Mechanic, Phone = "+992900000003" },
                new User { Email = "accountant@automaster.tj", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Account123!", 12), FullName = "Гулнора Сафарова", Role = UserRole.Accountant, Phone = "+992900000004" }
            );
        }

        var customerNames = new[] { "Хуршед Абдуллоев", "Зарина Юсупова", "Аброр Холов", "Манижа Раҳимова", "Бахтиёр Назаров",
            "Фарзона Қосимова", "Далер Раджабов", "Нигина Шарипова", "Умед Саидов", "Мадина Эргашева" };
        var customers = customerNames.Select((n, i) => new Customer
        {
            FullName = n,
            Phone = $"+99290010{i:D4}",
            Status = i < 2 ? CustomerStatus.Vip : CustomerStatus.Regular,
            TotalSpent = i < 2 ? 6000 + i * 500 : 500 * i
        }).ToList();
        db.Customers.AddRange(customers);

        var makes = new[] { ("Toyota", "Camry"), ("Toyota", "Corolla"), ("Chevrolet", "Lacetti"), ("Hyundai", "Sonata"), ("Hyundai", "Accent") };
        var vehicles = new List<Vehicle>();
        for (int i = 0; i < 15; i++)
        {
            var (make, model) = makes[i % makes.Length];
            vehicles.Add(new Vehicle
            {
                CustomerId = customers[i % customers.Count].Id,
                Make = make,
                Model = model,
                Year = 2015 + i % 9,
                LicensePlate = $"{1000 + i} AB 01",
                Mileage = 30000 + i * 5000
            });
        }
        db.Vehicles.AddRange(vehicles);

        var mechanics = new[]
        {
            new Mechanic { FullName = "Шерали Назаров", Specialization = MechanicSpecialization.Engine, Level = MechanicLevel.Senior, Salary = 8000, Rating = 4.7m },
            new Mechanic { FullName = "Comil Рустамов", Specialization = MechanicSpecialization.Suspension, Level = MechanicLevel.Middle, Salary = 6000, Rating = 4.3m },
            new Mechanic { FullName = "Парвиз Олимов", Specialization = MechanicSpecialization.Electrical, Level = MechanicLevel.Expert, Salary = 9500, Rating = 4.9m },
            new Mechanic { FullName = "Истам Юлдашев", Specialization = MechanicSpecialization.Body, Level = MechanicLevel.Junior, Salary = 4000, Rating = 3.9m },
            new Mechanic { FullName = "Рустам Назаров", Specialization = MechanicSpecialization.General, Level = MechanicLevel.Middle, Salary = 6500, Rating = 4.1m }
        };
        db.Mechanics.AddRange(mechanics);

        var branches = new[]
        {
            new Branch { Name = "AutoMaster — Центр", Address = "г. Душанбе, ул. Рудаки, 45", Phone = "+992370000001", Type = BranchType.Own },
            new Branch { Name = "AutoMaster — Сино", Address = "г. Душанбе, р-н Сино, ул. Айни, 12", Phone = "+992370000002", Type = BranchType.Franchise }
        };
        db.Branches.AddRange(branches);

        db.AppSettings.Add(new AppSettings
        {
            ServiceName = "AutoMaster CRM",
            Address = "г. Душанбе, ул. Рудаки, 45",
            Phone = "+992370000001",
            Currency = "TJS",
            Theme = "dark",
            Language = "ru"
        });

        await db.SaveChangesAsync();

        var statuses = new[] { OrderStatus.Intake, OrderStatus.Diagnosis, OrderStatus.Estimate, OrderStatus.Approved,
            OrderStatus.InProgress, OrderStatus.Waiting, OrderStatus.Ready, OrderStatus.Completed };
        var services = new[] { "Замена масла", "Ремонт подвески", "Диагностика двигателя", "Замена тормозных колодок", "Кузовной ремонт" };
        var rnd = new Random(42);
        var orders = new List<Order>();
        for (int i = 0; i < 20; i++)
        {
            var vehicle = vehicles[i % vehicles.Count];
            var status = statuses[i % statuses.Length];
            var labor = 200 + rnd.Next(0, 800);
            var parts = rnd.Next(0, 500);
            orders.Add(new Order
            {
                OrderNumber = $"ORD-{DateTime.UtcNow:yyMMdd}-{i + 1:D4}",
                CustomerId = vehicle.CustomerId,
                VehicleId = vehicle.Id,
                MechanicId = mechanics[i % mechanics.Length].Id,
                ServiceDescription = services[i % services.Length],
                Status = status,
                Priority = (OrderPriority)(i % 3),
                LaborCost = labor,
                PartsCost = parts,
                TotalCost = labor + parts,
                CreatedAt = DateTime.UtcNow.AddDays(-rnd.Next(0, 14)),
                CompletedAt = status == OrderStatus.Completed ? DateTime.UtcNow.AddDays(-rnd.Next(0, 7)) : null
            });
        }
        db.Orders.AddRange(orders);

        // История за 6 месяцев: даёт данные для прогноза выручки, тепловой карты и KPI.
        var historyServices = new[] { "Замена масла", "Замена масла", "Диагностика двигателя", "Замена тормозных колодок",
            "Ремонт подвески", "Электрика", "Кузовной ремонт", "Замена ремня ГРМ" };
        var peakHours = new[] { 9, 10, 10, 11, 11, 12, 14, 15, 15, 16, 17 };
        var nowLocal = DateTime.UtcNow.AddHours(5).Date;
        for (int i = 0; i < 180; i++)
        {
            var daysAgo = 15 + rnd.Next(0, 165);
            var day = nowLocal.AddDays(-daysAgo);
            if (day.DayOfWeek == DayOfWeek.Sunday) day = day.AddDays(1);
            var createdLocal = day.AddHours(peakHours[rnd.Next(peakHours.Length)]).AddMinutes(rnd.Next(0, 60));
            var createdUtc = DateTime.SpecifyKind(createdLocal.AddHours(-5), DateTimeKind.Utc);
            var vehicle = vehicles[rnd.Next(vehicles.Count)];
            // Рост бизнеса: чем ближе к сегодняшнему дню, тем выше средний чек.
            var growth = 1 + (165 - daysAgo) / 165.0 * 0.3;
            var labor = (decimal)Math.Round((150 + rnd.Next(0, 600)) * growth);
            var partsCost = rnd.Next(0, 400);
            orders.Add(new Order
            {
                OrderNumber = $"ORD-{createdLocal:yyMMdd}-H{i + 1:D3}",
                CustomerId = vehicle.CustomerId,
                VehicleId = vehicle.Id,
                MechanicId = mechanics[rnd.Next(mechanics.Length)].Id,
                ServiceDescription = historyServices[rnd.Next(historyServices.Length)],
                Status = OrderStatus.Completed,
                Priority = OrderPriority.Normal,
                LaborCost = labor,
                PartsCost = partsCost,
                TotalCost = labor + partsCost,
                CreatedAt = createdUtc,
                UpdatedAt = createdUtc.AddHours(2 + rnd.Next(0, 5)),
                CompletedAt = createdUtc.AddHours(2 + rnd.Next(0, 5))
            });
        }
        db.Orders.AddRange(orders.Skip(20));

        // Демо-клиент для личного кабинета (при сбросе данных пользователь уже существует)
        var clientUser = await db.Users.FirstOrDefaultAsync(u => u.Email == "client@automaster.tj");
        if (clientUser is null)
        {
            clientUser = new User
            {
                Email = "client@automaster.tj",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Client123!", 12),
                FullName = customers[0].FullName,
                Role = UserRole.Client,
                Phone = customers[0].Phone
            };
            db.Users.Add(clientUser);
        }
        customers[0].UserId = clientUser.Id;
        customers[0].LoyaltyPoints = 450;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.PromoCodes.AddRange(
            new PromoCode { Code = "NAVRUZ25", DiscountPercent = 25, ExpiresAt = today.AddMonths(3), UsedCount = 12 },
            new PromoCode { Code = "WELCOME10", DiscountPercent = 10, ExpiresAt = today.AddMonths(6), UsedCount = 34, MaxUses = 100 },
            new PromoCode { Code = "VIP15", DiscountPercent = 15, ExpiresAt = today.AddMonths(1), UsedCount = 5 });

        await db.SaveChangesAsync();
    }
}
