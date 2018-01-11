using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Services
{
    public interface IBudgetService : ICategoryService, IPayeeService, IAliasService, ITransactionService
    {
        
    }
}