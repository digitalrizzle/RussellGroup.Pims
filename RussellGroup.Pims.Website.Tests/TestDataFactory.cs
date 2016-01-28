using RussellGroup.Pims.DataAccess.Models;
using RussellGroup.Pims.Website.Models;
using System;
using System.Linq;

namespace RussellGroup.Pims.Website.Tests
{
    public static class TestDataFactory
    {
        public static IQueryable<Status> GetStatuses()
        {
            return new[]
            {
                new Status { Id = Status.Unknown, Name = "Unknown" },
                new Status { Id = Status.Available, Name = "Available" },
                new Status { Id = Status.CheckedOut, Name = "Checked out" },
                new Status { Id = Status.Stolen, Name = "Stolen" },
                new Status { Id = Status.UnderRepair, Name = "Under repair" },
                new Status { Id = Status.WrittenOff, Name = "Written off" }
            }.AsQueryable();
        }

        public static IQueryable<Condition> GetConditions()
        {
            return new[]
            {
                new Condition { Id = Condition.Unknown, Name = "Unknown" },
                new Condition { Id = Condition.Poor, Name = "Poor" },
                new Condition { Id = Condition.Fair, Name = "Fair" },
                new Condition { Id = Condition.Good, Name = "Good" },
                new Condition { Id = Condition.Excellent, Name = "Excellent" }
            }.AsQueryable();
        }

        public static IQueryable<Setting> GetSettings()
        {
            return new[]
            {
                new Setting { Key = "LastIssuedDocket", Value = "900001" }
            }.AsQueryable();
        }

        public static IQueryable<Category> GetCategories()
        {
            return new[]
            {
                new Category { Id = 1, Name = "Safety Equipment", Type = "Plant" },
                new Category { Id = 2, Name = "Saws", Type = "Plant" },
                new Category { Id = 3, Name = "Compressor", Type = "Plant" }
            }.AsQueryable();
        }

        public static IQueryable<Plant> GetPlants()
        {
            return new[]
            {
                new Plant
                {
                    Id = 1,
                    CategoryId = 1,
                    Category = GetCategories().Single(f => f.Id == 1),
                    StatusId = Status.Available,
                    Status = GetStatuses().Single(f => f.Id == Status.Available),
                    ConditionId = Condition.Excellent,
                    Condition = GetConditions().Single(f => f.Id == Condition.Excellent),
                    XPlantId = "01001",
                    XPlantNewId = "01001",
                    Description = "Safety Harness",
                    WhenPurchased = new DateTime(2010, 7, 31),
                    WhenDisused = null,
                    Rate = 2.35m,
                    Cost = 1500.00m,
                    Serial = null,
                    FixedAssetCode = null,
                    IsElectrical = false,
                    IsTool = false
                },
                new Plant
                {
                    Id = 2,
                    CategoryId = 2,
                    Category = GetCategories().Single(f => f.Id == 2),
                    StatusId = Status.Available,
                    Status = GetStatuses().Single(f => f.Id == Status.Available),
                    ConditionId = Condition.Good,
                    Condition = GetConditions().Single(f => f.Id == Condition.Good),
                    XPlantId = "02002",
                    XPlantNewId = "02002",
                    Description = "Saw",
                    WhenPurchased = new DateTime(2011, 3, 5),
                    WhenDisused = null,
                    Rate = 5.00m,
                    Cost = 650.50m,
                    Serial = "SAW02002",
                    FixedAssetCode = "DCL02002",
                    IsElectrical = true,
                    IsTool = true
                },
                new Plant
                {
                    Id = 3,
                    CategoryId = 2,
                    Category = GetCategories().Single(f => f.Id == 2),
                    StatusId = Status.CheckedOut,
                    Status = GetStatuses().Single(f => f.Id == Status.CheckedOut),
                    ConditionId = Condition.Poor,
                    Condition = GetConditions().Single(f => f.Id == Condition.Poor),
                    XPlantId = "03002",
                    XPlantNewId = "03002",
                    Description = "Saw",
                    WhenPurchased = new DateTime(2011, 11, 11),
                    WhenDisused = null,
                    Rate = 5.00m,
                    Cost = 650.50m,
                    Serial = "SAW03002",
                    FixedAssetCode = "DCL03002",
                    IsElectrical = true,
                    IsTool = true
                },
                new Plant
                {
                    Id = 4,
                    CategoryId = 3,
                    Category = GetCategories().Single(f => f.Id == 3),
                    StatusId = Status.Available,
                    Status = GetStatuses().Single(f => f.Id == Status.Available),
                    ConditionId = Condition.Fair,
                    Condition = GetConditions().Single(f => f.Id == Condition.Fair),
                    XPlantId = "04003",
                    XPlantNewId = "COMP004",
                    Description = "Compressor",
                    WhenPurchased = new DateTime(2009, 2, 28),
                    WhenDisused = null,
                    Rate = 0.50m,
                    Cost = 90.00m,
                    Serial = "COMP04003",
                    FixedAssetCode = null,
                    IsElectrical = false,
                    IsTool = false
                },
                new Plant
                {
                    Id = 5,
                    CategoryId = 3,
                    Category = GetCategories().Single(f => f.Id == 3),
                    StatusId = Status.UnderRepair,
                    Status = GetStatuses().Single(f => f.Id == Status.UnderRepair),
                    ConditionId = Condition.Poor,
                    Condition = GetConditions().Single(f => f.Id == Condition.Poor),
                    XPlantId = "05003",
                    XPlantNewId = "05003",
                    Description = "Compressor",
                    WhenPurchased = new DateTime(2009, 9, 1),
                    WhenDisused = null,
                    Rate = 23.99m,
                    Cost = 2450.00m,
                    Serial = null,
                    FixedAssetCode = "DCL05003",
                    IsElectrical = true,
                    IsTool = false
                },
                new Plant
                {
                    Id = 6,
                    CategoryId = 3,
                    Category = GetCategories().Single(f => f.Id == 3),
                    StatusId = Status.CheckedOut,
                    Status = GetStatuses().Single(f => f.Id == Status.CheckedOut),
                    ConditionId = Condition.Poor,
                    Condition = GetConditions().Single(f => f.Id == Condition.Unknown),
                    XPlantId = "CMPSSR1",
                    XPlantNewId = "06003",
                    Description = "Compressor",
                    WhenPurchased = new DateTime(2012, 2, 28),
                    WhenDisused = null,
                    Rate = 23.99m,
                    Cost = 2450.00m,
                    Serial = "COMP06003",
                    FixedAssetCode = "DCL06003",
                    IsElectrical = true,
                    IsTool = false
                }
            }.AsQueryable();
        }

        public static IQueryable<PlantHire> GetPlantHires()
        {
            var plantHires = GetJobs(hasPlantHires: true)
                .Where(f => f.PlantHires != null)
                .SelectMany(f => f.PlantHires);

            return plantHires;
        }

        private static PlantHire[] _getPlantHires()
        {
            var job1 = GetJobs(hasPlantHires: false).Single(f => f.XJobId.Equals("DC0001"));

            var plant2 = GetPlants().Single(f => f.XPlantId.Equals("02002"));
            var plant3 = GetPlants().Single(f => f.XPlantId.Equals("03002"));
            var plant6 = GetPlants().Single(f => f.XPlantId.Equals("CMPSSR1"));

            var plantHire1 = new PlantHire
            {
                Id = 1,
                PlantId = plant2.Id,
                Plant = plant2,
                Docket = "123456",
                WhenStarted = new DateTime(2015, 12, 31),
                WhenEnded = new DateTime(2016, 1, 15),
                Rate = plant2.Rate,
                Job = job1,
                JobId = job1.Id
            };

            var plantHire2 = new PlantHire
            {
                Id = 2,
                PlantId = plant3.Id,
                Plant = plant3,
                Docket = "123456",
                WhenStarted = new DateTime(2015, 12, 31),
                Rate = plant3.Rate,
                Job = job1,
                JobId = job1.Id
            };

            var plantHire3 = new PlantHire
            {
                Id = 3,
                PlantId = plant6.Id,
                Plant = plant6,
                Docket = "123456",
                WhenStarted = new DateTime(2015, 12, 31),
                WhenEnded = new DateTime(2015, 12, 31),
                Rate = plant6.Rate,
                Job = job1,
                JobId = job1.Id
            };

            var plantHire4 = new PlantHire
            {
                Id = 4,
                PlantId = plant6.Id,
                Plant = plant6,
                Docket = "123456",
                WhenStarted = new DateTime(2015, 12, 1),
                Rate = plant6.Rate,
                Job = job1,
                JobId = job1.Id
            };

            var plantHires = new[]
            {
                plantHire1,
                plantHire2,
                plantHire3,
                plantHire4
            };

            plant2.PlantHires = new[] { plantHire1 };
            plant3.PlantHires = new[] { plantHire2 };
            plant6.PlantHires = new[] { plantHire3, plantHire4 };

            return plantHires;
        }

        public static IQueryable<Job> GetJobs(bool hasPlantHires = false)
        {
            return new[]
            {
                new Job
                {
                    Id = 1,
                    XJobId = "DC0001",
                    Description = "First Test Job",
                    WhenStarted = new DateTime(2015, 12, 1),
                    WhenEnded = new DateTime(2015, 12, 31),
                    ProjectManager = "Orlando Gee",
                    QuantitySurveyor = "Jarrod Koonce",
                    Comment = "This is a comment.",
                    PlantHires = hasPlantHires ? _getPlantHires() : new PlantHire[0]
                },
                new Job
                {
                    Id = 2,
                    XJobId = "DC0002",
                    Description = "Second Test Job",
                    WhenStarted = new DateTime(2016, 3, 1),
                    WhenEnded = new DateTime(2016, 9, 30),
                    ProjectManager = "Peter Parker",
                    QuantitySurveyor = "Johnathan Morefield",
                    Comment = "This is another comment."
                },
                new Job
                {
                    Id = 3,
                    XJobId = "XX9999",
                    Description = "Random Test Job",
                    WhenStarted = new DateTime(2031, 1, 1),
                    WhenEnded = new DateTime(2031, 12, 31),
                    ProjectManager = "Carlee Walquist",
                    QuantitySurveyor = "Susan James",
                    Comment = "This is yet another comment."
                }
            }.AsQueryable();
        }

        public static BatchCheckout GetBatchCheckout()
        {
            return new BatchCheckout
            {
                WhenStarted = new DateTime(2015, 12, 31),
                Scans = $"DC0001{Environment.NewLine}01001{Environment.NewLine}02002{Environment.NewLine}99904003"
            };
        }

        public static BatchCheckin GetBatchCheckin()
        {
            return new BatchCheckin
            {
                WhenEnded = new DateTime(2017, 10, 31),
                Scans = $"DOCKET{Environment.NewLine}123456{Environment.NewLine}03002{Environment.NewLine}99906003"
            };
        }
    }
}
