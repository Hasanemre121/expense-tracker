using ExpenseTracker.Models;

namespace ExpenseTracker.Services;

public interface IExpenseService
{
    Expense Add(string description, decimal amount, Category category, DateTime date);
    bool Delete(int id);
    IReadOnlyList<Expense> GetAll();
    IReadOnlyList<Expense> Filter(Category? category, DateTime? from, DateTime? to);
    IReadOnlyDictionary<Category, decimal> TotalsByCategory(int year, int month);
    decimal MonthlyTotal(int year, int month);
    IReadOnlyList<(int Year, int Month, decimal Total)> MonthlyTrend(int lastMonths, DateTime today);
    string ExportCsv();
}
