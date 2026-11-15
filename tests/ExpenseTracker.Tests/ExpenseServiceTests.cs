using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Tests;

public class ExpenseServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ExpenseDbContext _db;
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<ExpenseDbContext>().UseSqlite(_connection).Options;
        _db = new ExpenseDbContext(options);
        _db.Database.EnsureCreated();
        _service = new ExpenseService(_db);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public void Add_ValidExpense_IsPersisted()
    {
        var e = _service.Add("Lunch", 85.5m, Category.Food, new DateTime(2026, 11, 1));
        Assert.True(e.Id > 0);
        Assert.Single(_service.GetAll());
    }

    [Theory]
    [InlineData("", 10)]
    [InlineData("   ", 10)]
    public void Add_EmptyDescription_Throws(string description, decimal amount) =>
        Assert.Throws<ArgumentException>(() => _service.Add(description, amount, Category.Other, DateTime.Today));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Add_NonPositiveAmount_Throws(decimal amount) =>
        Assert.Throws<ArgumentOutOfRangeException>(() => _service.Add("Test", amount, Category.Other, DateTime.Today));

    [Fact]
    public void Delete_ExistingId_ReturnsTrue_AndMissingReturnsFalse()
    {
        var e = _service.Add("Bus", 20m, Category.Transport, DateTime.Today);
        Assert.True(_service.Delete(e.Id));
        Assert.False(_service.Delete(e.Id));
    }

    [Fact]
    public void Filter_ByCategoryAndDateRange_ReturnsMatchesOnly()
    {
        _service.Add("Lunch", 50m, Category.Food, new DateTime(2026, 11, 2));
        _service.Add("Dinner", 70m, Category.Food, new DateTime(2026, 10, 2));
        _service.Add("Metro", 15m, Category.Transport, new DateTime(2026, 11, 3));

        var result = _service.Filter(Category.Food, new DateTime(2026, 11, 1), new DateTime(2026, 11, 30));

        Assert.Single(result);
        Assert.Equal("Lunch", result[0].Description);
    }

    [Fact]
    public void TotalsByCategory_SumsPerCategoryForMonth()
    {
        _service.Add("A", 10m, Category.Food, new DateTime(2026, 11, 1));
        _service.Add("B", 15.5m, Category.Food, new DateTime(2026, 11, 20));
        _service.Add("C", 40m, Category.Bills, new DateTime(2026, 11, 5));
        _service.Add("D", 99m, Category.Bills, new DateTime(2026, 10, 5));

        var totals = _service.TotalsByCategory(2026, 11);

        Assert.Equal(25.5m, totals[Category.Food]);
        Assert.Equal(40m, totals[Category.Bills]);
        Assert.Equal(65.5m, _service.MonthlyTotal(2026, 11));
    }

    [Fact]
    public void MonthlyTrend_ReturnsRequestedNumberOfMonths_OldestFirst()
    {
        _service.Add("A", 10m, Category.Food, new DateTime(2026, 9, 1));
        var trend = _service.MonthlyTrend(3, new DateTime(2026, 11, 15));

        Assert.Equal(3, trend.Count);
        Assert.Equal((2026, 9, 10m), trend[0]);
        Assert.Equal((2026, 11, 0m), trend[2]);
    }

    [Fact]
    public void ExportCsv_EscapesQuotesInDescription()
    {
        _service.Add("Book \"C# in Depth\"", 300m, Category.Education, new DateTime(2026, 11, 1));
        var csv = _service.ExportCsv();

        Assert.StartsWith("Id,Date,Category,Description,Amount", csv);
        Assert.Contains("\"Book \"\"C# in Depth\"\"\"", csv);
        Assert.Contains("300.00", csv);
    }
}
