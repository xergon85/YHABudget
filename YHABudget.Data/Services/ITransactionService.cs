using YHABudget.Data.Enums;
using YHABudget.Data.Models;

namespace YHABudget.Data.Services;

public interface ITransactionService
{
    Transaction AddTransaction(Transaction transaction);
    IEnumerable<Transaction> GetAllTransactions();
    Transaction? GetTransactionById(int id);
    IEnumerable<Transaction> GetTransactionsByMonth(DateTime month);
    IEnumerable<Transaction> GetTransactionsByFilter(TransactionType? type, int? categoryId, DateTime? month);
    IEnumerable<DateTime> GetMonthsWithTransactions();
    void UpdateTransaction(Transaction transaction);
    void DeleteTransaction(int id);
}
