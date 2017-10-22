using ExpenseTracker.Models;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Tests
{
    public static class TestInitializer
    {
        public static List<BudgetCategory> CreateTestCategories() {
            List<BudgetCategory> categories = new List<BudgetCategory> {
                new BudgetCategory {
                    ID = 1,
                    Name = "Rent",
                    Amount = 575,
                    BeginEffectiveDate = new DateTime(2015, 02, 01),
                    EndEffectiveDate = null,
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 2,
                    Name = "Groceries",
                    Amount = 750,
                    BeginEffectiveDate = new DateTime(2017, 10, 01),
                    EndEffectiveDate = null,
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "Old Groceries",
                    Amount = 700,
                    BeginEffectiveDate = new DateTime(2016, 10, 01),
                    EndEffectiveDate = new DateTime(2017, 09, 30),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 4,
                    Name = "ARC Income",
                    Amount = 0,
                    BeginEffectiveDate = new DateTime(2016, 06, 27),
                    EndEffectiveDate = null,
                    Type = BudgetType.Income
                }
            };

            return categories;
        }
    }
}