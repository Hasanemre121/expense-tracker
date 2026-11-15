# Expense Tracker

A console application for tracking personal expenses, built with **C# / .NET 10**, **Entity Framework Core** and **SQLite**.

## Features
- Add, list, filter and delete expenses
- Categories (Food, Transport, Education, Entertainment, Health, Bills, Other)
- Monthly report with per-category totals, percentage share and a text bar chart
- Six-month spending trend
- CSV export (with proper escaping)
- Input validation and clear, colored console output
- Data persisted locally in a SQLite database

## Project structure
```
src/ExpenseTracker
  Models/     Expense entity and Category enum
  Data/       EF Core DbContext (SQLite)
  Services/   IExpenseService + ExpenseService (business logic, LINQ queries)
  Program.cs  Console menu and user input handling
tests/ExpenseTracker.Tests
  ExpenseServiceTests.cs   xUnit tests (in-memory SQLite)
```

## Concepts practiced
- Object-oriented design and separation of concerns (UI vs. business logic vs. data)
- Interfaces for testability
- LINQ (filtering, grouping, aggregation)
- EF Core with SQLite
- Unit testing with xUnit

## Getting started
Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/Hasanemre121/expense-tracker.git
cd expense-tracker
dotnet run --project src/ExpenseTracker
```

Run the tests:
```bash
dotnet test
```

## About this project
This is a learning project. It was developed with the help of an AI assistant while I study C#, and I am working through the code to understand each part. Planned next steps: a web API layer, budget limits per category, and a data-analysis script for spending patterns.
