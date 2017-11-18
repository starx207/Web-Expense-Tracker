using ExpenseTracker.Data.Repository;
using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IBudget context) {
            if (context.GetCategories().Any()) {
                return; // Db Already has data
            }

            List<BudgetCategory> categories = new List<BudgetCategory> {
                new BudgetCategory {
                    Name = "Electric Bill", 
                    Amount = 100, 
                    BeginEffectiveDate = DateTime.Parse("9/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Fuel",
                    Amount = 150,
                    BeginEffectiveDate = DateTime.Parse("9/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Gas Bill",
                    Amount = 65,
                    BeginEffectiveDate = DateTime.Parse("8/1/2017"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Groceries",
                    Amount = 750,
                    BeginEffectiveDate = DateTime.Parse("10/1/2017"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Insurance",
                    Amount = 650,
                    BeginEffectiveDate = DateTime.Parse("10/1/2017"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Internet",
                    Amount = 50,
                    BeginEffectiveDate = DateTime.Parse("5/1/2017"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Phone",
                    Amount = 95,
                    BeginEffectiveDate = DateTime.Parse("6/1/2017"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Student Loans",
                    Amount = 500,
                    BeginEffectiveDate = DateTime.Parse("6/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Tithe",
                    Amount = 563,
                    BeginEffectiveDate = DateTime.Parse("10/1/2017"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Water Bill",
                    Amount = 110,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Rent",
                    Amount = 575,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Extra Spending",
                    Amount = 350,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                }
            };

            foreach (var c in categories) {
                context.AddBudgetCategory(c);
            }

            context.SaveChangesAsync();
        }
    }
}