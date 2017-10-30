using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                    Name = "Income",
                    Amount = 0,
                    BeginEffectiveDate = new DateTime(2016, 06, 27),
                    EndEffectiveDate = null,
                    Type = BudgetType.Income
                }
            };

            return categories;
        }

        public static List<Payee> CreateTestPayees(IQueryable<BudgetCategory> categories) {
            List<Payee> payees = new List<Payee> {
                new Payee {
                    ID = 1,
                    Name = "Appraisal Research Corp",
                    BeginEffectiveDate = new DateTime(2016, 7, 1),
                    EndEffectiveDate = null,
                    BudgetCategoryID = 4,
                    Category = categories.Where(c => c.ID == 4).FirstOrDefault()
                },
                new Payee {
                    ID = 2,
                    Name = "Drum Lessons",
                    BeginEffectiveDate = new DateTime(2017, 4, 1),
                    EndEffectiveDate = null,
                    BudgetCategoryID = 4,
                    Category = categories.Where(c => c.ID == 4).FirstOrDefault()
                },
                new Payee {
                    ID = 3,
                    Name = "Kroger",
                    BeginEffectiveDate = new DateTime(2014, 6, 14),
                    EndEffectiveDate = null,
                    BudgetCategoryID = 2,
                    Category = categories.Where(c => c.ID == 2).FirstOrDefault()
                } // Add more payees for testing
            };

            return payees;
        }
    }
}