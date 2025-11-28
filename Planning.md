# Plan: WPF Budget Planner Application

Build a desktop WPF budget planner with MVVM architecture that manages income/expenses with categories, supports recurring and one-time transactions, calculates monthly projections, persists data locally, and optionally handles absence days (VAB/sick) with automatic salary adjustments.

## Steps

1. **Initialize project structure** — Create solution with WPF project targeting .NET 10, add NuGet packages (Entity Framework Core, SQLite provider, CommunityToolkit.Mvvm), and set up folder structure for Models/ViewModels/Views/Data/Services/Commands/Resources.

2. **Create data models** — Define `Transaction` model with Type property (Income/Expense enum), `RecurringTransaction` to manage recurring patterns, `Category`, `RecurrenceType` enum (None/Monthly/Yearly), and optionally `Absence` model. Transaction properties: Id, Amount (positive for income, negative for expense or use Type + absolute value), Description, Date, CategoryId, IsRecurring flag. RecurringTransaction properties: Id, TemplateTransactionId, RecurrenceType, RecurrenceMonth (for yearly), StartDate, EndDate (optional).

3. **Set up database layer** — Implement `BudgetDbContext` with DbSets, configure Entity Framework Core with SQLite, seed initial categories (Mat, Hus & drift, Transport, Fritid, etc.), and create database initialization logic.

4. **Build ViewModels with MVVM** — Create `MainViewModel`, `TransactionViewModel` (handles both income and expense with filtering/grouping by Type), `RecurringTransactionViewModel`, and `ProjectionViewModel` with ObservableCollections, ICommand properties using RelayCommand, implement INotifyPropertyChanged, and add business logic for CRUD operations and calculations.

5. **Design XAML views** — Build `MainWindow.xaml` with tab navigation, create `TransactionView.xaml` (with toggle/filter for Income vs Expense display), `RecurringTransactionView.xaml`, and `ProjectionView.xaml` with data-bound controls (DataGrid/ListBox for lists, forms for add/edit), apply DataTemplates, Styles, and Resources for consistent UI, and bind to ViewModels.

6. **Implement calculation services** — Create `CalculationService` for monthly income calculation (annual income ÷ annual hours), projection logic (sum recurring + one-time transactions), and optionally absence deduction/compensation logic (80% return with 7.5 PBB cap at 410,000 kr for VAB).

## Further Considerations

1. **Database choice** — SQLite recommended for simpler deployment (single file, no server setup) vs MS SQL Server if planning multi-user scenarios or already familiar with SQL Server tooling?

2. **Extra feature scope** — Implement absence (VAB/sick days) functionality from start, or build core features first then add as phase 2?

3. **Testing strategy** — Include unit tests for calculation logic (`CalculationService`) and ViewModel behavior, or rely on manual UI testing during development?
