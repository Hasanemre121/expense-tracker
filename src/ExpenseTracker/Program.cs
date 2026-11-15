using System.Globalization;
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.Services;
using Microsoft.EntityFrameworkCore;

var dbPath = Path.Combine(AppContext.BaseDirectory, "expenses.db");
var options = new DbContextOptionsBuilder<ExpenseDbContext>()
    .UseSqlite($"Data Source={dbPath}")
    .Options;

using var db = new ExpenseDbContext(options);
db.Database.EnsureCreated();
IExpenseService service = new ExpenseService(db);

while (true)
{
    Console.Clear();
    WriteColored("=== Expense Tracker ===", ConsoleColor.Cyan);
    Console.WriteLine("1) Add expense");
    Console.WriteLine("2) List expenses");
    Console.WriteLine("3) Filter expenses");
    Console.WriteLine("4) Delete expense");
    Console.WriteLine("5) Monthly report");
    Console.WriteLine("6) Export to CSV");
    Console.WriteLine("0) Exit");
    Console.Write("\nChoose: ");

    switch (Console.ReadLine()?.Trim())
    {
        case "1": AddExpense(); break;
        case "2": PrintTable(service.GetAll()); break;
        case "3": FilterExpenses(); break;
        case "4": DeleteExpense(); break;
        case "5": MonthlyReport(); break;
        case "6": Export(); break;
        case "0": return;
        default: WriteColored("Invalid option.", ConsoleColor.Red); break;
    }
    Console.WriteLine("\nPress Enter to continue...");
    Console.ReadLine();
}

void AddExpense()
{
    Console.Write("Description: ");
    var description = Console.ReadLine() ?? "";
    var amount = ReadDecimal("Amount: ");
    var category = ReadCategory();
    var date = ReadDate("Date (yyyy-MM-dd, empty = today): ") ?? DateTime.Today;
    try
    {
        var e = service.Add(description, amount, category, date);
        WriteColored($"Saved expense #{e.Id}.", ConsoleColor.Green);
    }
    catch (Exception ex) when (ex is ArgumentException)
    {
        WriteColored(ex.Message, ConsoleColor.Red);
    }
}

void FilterExpenses()
{
    Console.Write("Category (empty = any): ");
    Category? category = Enum.TryParse<Category>(Console.ReadLine(), true, out var c) ? c : null;
    var from = ReadDate("From (yyyy-MM-dd, empty = any): ");
    var to = ReadDate("To (yyyy-MM-dd, empty = any): ");
    PrintTable(service.Filter(category, from, to));
}

void DeleteExpense()
{
    Console.Write("Expense id: ");
    if (int.TryParse(Console.ReadLine(), out var id) && service.Delete(id))
        WriteColored("Deleted.", ConsoleColor.Green);
    else
        WriteColored("Expense not found.", ConsoleColor.Red);
}

void MonthlyReport()
{
    var today = DateTime.Today;
    var totals = service.TotalsByCategory(today.Year, today.Month);
    var total = service.MonthlyTotal(today.Year, today.Month);
    WriteColored($"\nReport for {today:MMMM yyyy}", ConsoleColor.Cyan);
    foreach (var (cat, sum) in totals.OrderByDescending(t => t.Value))
    {
        var share = total == 0 ? 0 : sum / total * 100;
        Console.WriteLine($"{cat,-14}{sum,10:F2}  {share,5:F1}%  {new string('#', (int)(share / 5))}");
    }
    Console.WriteLine($"{"TOTAL",-14}{total,10:F2}");

    WriteColored("\nLast 6 months", ConsoleColor.Cyan);
    foreach (var (y, m, t) in service.MonthlyTrend(6, today))
        Console.WriteLine($"{y}-{m:00}  {t,10:F2}");
}

void Export()
{
    var path = Path.Combine(Directory.GetCurrentDirectory(), "expenses.csv");
    File.WriteAllText(path, service.ExportCsv());
    WriteColored($"Exported to {path}", ConsoleColor.Green);
}

void PrintTable(IReadOnlyList<Expense> items)
{
    if (items.Count == 0) { WriteColored("No expenses found.", ConsoleColor.Yellow); return; }
    Console.WriteLine($"\n{"Id",-4}{"Date",-12}{"Category",-14}{"Amount",10}  Description");
    foreach (var e in items)
        Console.WriteLine($"{e.Id,-4}{e.Date:yyyy-MM-dd,-12}{e.Category,-14}{e.Amount,10:F2}  {e.Description}");
    Console.WriteLine($"\nTotal: {items.Sum(e => e.Amount):F2}");
}

decimal ReadDecimal(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var text = (Console.ReadLine() ?? "").Replace(',', '.');
        if (decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) && v > 0) return v;
        WriteColored("Enter a positive number.", ConsoleColor.Red);
    }
}

Category ReadCategory()
{
    Console.WriteLine("Categories: " + string.Join(", ", Enum.GetNames<Category>()));
    while (true)
    {
        Console.Write("Category: ");
        if (Enum.TryParse<Category>(Console.ReadLine(), true, out var c)) return c;
        WriteColored("Unknown category.", ConsoleColor.Red);
    }
}

DateTime? ReadDate(string prompt)
{
    while (true)
    {
        Console.Write(prompt);
        var text = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(text)) return null;
        if (DateTime.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)) return d;
        WriteColored("Use the format yyyy-MM-dd.", ConsoleColor.Red);
    }
}

void WriteColored(string text, ConsoleColor color)
{
    Console.ForegroundColor = color;
    Console.WriteLine(text);
    Console.ResetColor();
}

public partial class Program { }
