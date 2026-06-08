using GLMS.API.Models;
using System.Security.Cryptography;
using System.Text;

namespace GLMS.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(GLMSDbContext context)
        {
            if (context.Users.Any()) return; // Already seeded

            // Seed admin user
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@techmove.com",
                PasswordHash = HashPassword("Admin123!"),
                Role = "Admin",
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true
            };

            var managerUser = new User
            {
                Username = "manager",
                Email = "manager@techmove.com",
                PasswordHash = HashPassword("Manager123!"),
                Role = "Manager",
                FirstName = "Logistics",
                LastName = "Manager",
                IsActive = true
            };

            var user = new User
            {
                Username = "user",
                Email = "user@techmove.com",
                PasswordHash = HashPassword("User123!"),
                Role = "User",
                FirstName = "Regular",
                LastName = "User",
                IsActive = true
            };

            context.Users.AddRange(adminUser, managerUser, user);

            // Seed sample contracts
            var contracts = new List<Contract>
            {
                new Contract
                {
                    ContractNumber = "CNT-2024-001",
                    ClientName = "Global Shipping Inc.",
                    ClientEmail = "contact@globalshipping.com",
                    ClientPhone = "+1-555-0101",
                    StartDate = DateTime.UtcNow.AddMonths(-2),
                    EndDate = DateTime.UtcNow.AddMonths(10),
                    ContractValue = 75000.00m,
                    Currency = "USD",
                    Status = ContractStatus.Active,
                    Type = ContractType.Freight,
                    Description = "International freight forwarding services for Asia-Pacific routes",
                    ServiceLevelAgreement = "99.5% on-time delivery, 24/7 tracking support",
                    CreatedBy = "admin"
                },
                new Contract
                {
                    ContractNumber = "CNT-2024-002",
                    ClientName = "EuroWarehousing Ltd.",
                    ClientEmail = "operations@eurowarehousing.eu",
                    ClientPhone = "+49-555-0202",
                    StartDate = DateTime.UtcNow.AddMonths(-1),
                    EndDate = DateTime.UtcNow.AddMonths(23),
                    ContractValue = 120000.00m,
                    Currency = "EUR",
                    Status = ContractStatus.Active,
                    Type = ContractType.Warehousing,
                    Description = "Cold storage warehousing in Frankfurt and Munich facilities",
                    ServiceLevelAgreement = "Temperature monitoring every 15 minutes, 99.9% inventory accuracy",
                    CreatedBy = "admin"
                },
                new Contract
                {
                    ContractNumber = "CNT-2024-003",
                    ClientName = "Express Delivery Co.",
                    ClientEmail = "logistics@expressdelivery.com",
                    ClientPhone = "+1-555-0303",
                    StartDate = DateTime.UtcNow.AddDays(-5),
                    EndDate = DateTime.UtcNow.AddMonths(6),
                    ContractValue = 25000.00m,
                    Currency = "USD",
                    Status = ContractStatus.Draft,
                    Type = ContractType.ExpressDelivery,
                    Description = "Same-day express delivery services in metropolitan areas",
                    ServiceLevelAgreement = "2-hour delivery window, real-time GPS tracking",
                    CreatedBy = "manager"
                },
                new Contract
                {
                    ContractNumber = "CNT-2024-004",
                    ClientName = "Pacific Trade Partners",
                    ClientEmail = "shipping@pacifictrade.com",
                    ClientPhone = "+61-555-0404",
                    StartDate = DateTime.UtcNow.AddMonths(-6),
                    EndDate = DateTime.UtcNow.AddDays(-2), // Expired
                    ContractValue = 95000.00m,
                    Currency = "AUD",
                    Status = ContractStatus.Expired,
                    Type = ContractType.Freight,
                    Description = "Ocean freight services between Australia and North America",
                    ServiceLevelAgreement = "Weekly sailings, cargo insurance included",
                    CreatedBy = "admin"
                },
                new Contract
                {
                    ContractNumber = "CNT-2024-005",
                    ClientName = "Nordic Logistics AB",
                    ClientEmail = "contact@nordiclogistics.se",
                    ClientPhone = "+46-555-0505",
                    StartDate = DateTime.UtcNow.AddMonths(-3),
                    EndDate = DateTime.UtcNow.AddMonths(9),
                    ContractValue = 45000.00m,
                    Currency = "SEK",
                    Status = ContractStatus.OnHold,
                    Type = ContractType.Freight,
                    Description = "Scandinavian freight consolidation services",
                    ServiceLevelAgreement = "48-hour delivery within Scandinavia",
                    CreatedBy = "manager"
                }
            };

            context.Contracts.AddRange(contracts);

            // Seed sample service requests
            var serviceRequests = new List<ServiceRequest>
            {
                new ServiceRequest
                {
                    RequestNumber = "SR-2024-00001",
                    ContractId = contracts[0].Id, // Global Shipping
                    Type = ServiceType.Freight,
                    Status = RequestStatus.Completed,
                    Description = "Container shipment from Shanghai to Los Angeles",
                    EstimatedCost = 8500.00m,
                    ActualCost = 8200.00m,
                    RequestDate = DateTime.UtcNow.AddDays(-30),
                    ScheduledDate = DateTime.UtcNow.AddDays(-25),
                    CompletionDate = DateTime.UtcNow.AddDays(-20),
                    PickupLocation = "Shanghai Port, China",
                    DeliveryLocation = "Port of Los Angeles, USA"
                },
                new ServiceRequest
                {
                    RequestNumber = "SR-2024-00002",
                    ContractId = contracts[0].Id, // Global Shipping
                    Type = ServiceType.Freight,
                    Status = RequestStatus.InProgress,
                    Description = "Air freight from Tokyo to London",
                    EstimatedCost = 12000.00m,
                    RequestDate = DateTime.UtcNow.AddDays(-5),
                    ScheduledDate = DateTime.UtcNow.AddDays(2),
                    PickupLocation = "Narita Airport, Tokyo",
                    DeliveryLocation = "Heathrow Airport, London"
                },
                new ServiceRequest
                {
                    RequestNumber = "SR-2024-00003",
                    ContractId = contracts[1].Id, // EuroWarehousing
                    Type = ServiceType.Warehousing,
                    Status = RequestStatus.Approved,
                    Description = "Pharmaceutical storage batch #PH-2024-001",
                    EstimatedCost = 3500.00m,
                    RequestDate = DateTime.UtcNow.AddDays(-2),
                    ScheduledDate = DateTime.UtcNow.AddDays(1),
                    PickupLocation = "Bayer Pharma, Berlin",
                    DeliveryLocation = "Frankfurt Cold Storage Facility"
                },
                new ServiceRequest
                {
                    RequestNumber = "SR-2024-00004",
                    ContractId = contracts[0].Id, // Global Shipping
                    Type = ServiceType.Freight,
                    Status = RequestStatus.Pending,
                    Description = "Bulk grain shipment from Buenos Aires to Rotterdam",
                    EstimatedCost = 15000.00m,
                    RequestDate = DateTime.UtcNow,
                    ScheduledDate = DateTime.UtcNow.AddDays(7),
                    PickupLocation = "Port of Buenos Aires, Argentina",
                    DeliveryLocation = "Port of Rotterdam, Netherlands"
                }
            };

            context.ServiceRequests.AddRange(serviceRequests);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}