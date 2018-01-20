using ExpenseTracker.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Repository
{
    public interface IDataRepo : ICategoryRepo, IPayeeRepo, IAliasRepo, ITransactionRepo
    {
        
    }
}