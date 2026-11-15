using System.Globalization;
using System.Text;
using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

public class ExpenseService : IExpenseService
{
    private readonly ExpenseDbContext _db;

    public ExpenseService(ExpenseDbContext db) => _db = db;

    public Expense Add(string description, decimal amount, Category category, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be empty.", nameof(description));
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be greater than zero.");

        var expense = new Expense
        {
            Description = description.Trim(),
            Amount = amount,
            Category = category,
            Date = date.Date
        };
        _db.Expenses.Add(expense);
        _db.SaveChanges();
        return expense;
    }

    public bool Delete(int id)
    {
        var expense = _db.Expenses.Find(id);
        if (expense is null) return false;
        _db.Expenses.Remove(expense);
        _db.SaveChanges();
        return true;
    }

    public IReadOnlyList<Expense> GetAll() =>
        _db.Expenses.AsEnumerable().OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).ToList();

    public IReadOnlyList<Expense> Filter(Category? category, DateTime? from, DateTime? to)
    {
        // Filtering happens in memory: SQLite stores decimals/dates in ways that EF cannot always translate.
        IEnumerable<Expense> query = _db.Expenses.AsEnumerable();
        if (category.HasValue) query = query.Where(e => e.Category == category.Value);
        if (from.HasValue) query = query.Where(e => e.Date >= from.Value.Date);
        if (to.HasValue) query = query.Where(e => e.Date <= to.Value.Date);
        return query.OrderByDescending(e => e.Date).ThenByDescending(e => e.Id).ToList();
    }

    public IReadOnlyDictionary<Category, decimal> TotalsByCategory(int year, int month) =>
        _db.Expenses.AsEnumerable()
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .GroupBy(e => e.Category)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

    public decimal MonthlyTotal(int year, int month) =>
        _db.Expenses.AsEnumerable()
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .Sum(e => e.Amount);

    public IReadOnlyList<(int Year, int Month, decimal Total)> MonthlyTrend(int lastMonths, DateTime today)
    {
        var result = new List<(int, int, decimal)>();
        for (int i = lastMonths - 1; i >= 0; i--)
        {
            var d = new DateTime(today.Year, today.Month, 1).AddMonths(-i);
            result.Add((d.Year, d.Month, MonthlyTotal(d.Year, d.Month)));
        }
        return result;
    }

    public string ExportCsv()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Id,Date,Category,Description,Amount");
        foreach (var e in GetAll())
        {
            var description = "\"" + e.Description.Replace("\"", "\"\"") + "\"";
            sb.Append(e.Id).Append(',')
              .Append(e.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)).Append(',')
              .Append(e.Category).Append(',')
              .Append(description).Append(',')
              .Append(e.Amount.ToString("F2", CultureInfo.InvariantCulture))
              .AppendLine();
        }
        return sb.ToString();
    }
}
