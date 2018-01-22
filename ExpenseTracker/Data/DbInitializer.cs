using ExpenseTracker.Repository;
using ExpenseTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(IBudgetRepo context) {
            if (context.GetCategories().Any()) {
                return; // Db Already has data
            }

            // Populate Budget Categories
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
                },
                new BudgetCategory {
                    Name = "ARC Income",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Income
                },
                new BudgetCategory {
                    Name = "ATM Withdrawals",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Medical",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Restaurants",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Miscellaneous",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Shopping",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Vehicle Maintenance",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Entertainment",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Taxes",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    Name = "Tax Refund",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Income
                },
                new BudgetCategory {
                    Name = "Teaching Drums",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("5/1/2016"),
                    Type = BudgetType.Income
                },
                new BudgetCategory {
                    Name = "Extra Income",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    Type = BudgetType.Income
                },
                new BudgetCategory {
                    Name = "Tutoring",
                    Amount = 0,
                    BeginEffectiveDate = DateTime.Parse("10/1/2017"),
                    Type = BudgetType.Income
                }
            };

            foreach (var c in categories) {
                context.AddBudgetCategory(c);
            }

            // Populate Payees
            List<Payee> payees = new List<Payee> {
                new Payee {
                    Name = "Appraisal Research",
                    BeginEffectiveDate = DateTime.Parse("6/20/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "ARC Income").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "ARC Income").FirstOrDefault()
                },
                new Payee {
                    Name = "Cash Deposit",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Extra Income").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Extra Income").FirstOrDefault()
                },
                new Payee {
                    Name = "Earned Interest",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Extra Income").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Extra Income").FirstOrDefault()
                },
                new Payee {
                    Name = "Reiske Drum Lesson",
                    BeginEffectiveDate = DateTime.Parse("5/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Teaching Drums").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Teaching Drums").FirstOrDefault()
                },
                new Payee {
                    Name = "Northwestern Water (102 Maple)",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Water Bill").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Water Bill").FirstOrDefault()
                },
                new Payee {
                    Name = "Medical Mutual",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Insurance").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Insurance").FirstOrDefault()
                },
                new Payee {
                    Name = "Dave and Louise",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Rent").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Rent").FirstOrDefault()
                },
                new Payee {
                    Name = "Verizon Wireless",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Phone").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Phone").FirstOrDefault()
                },
                new Payee {
                    Name = "Columbia Gas of Ohio",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Gas Bill").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Gas Bill").FirstOrDefault()
                },
                new Payee {
                    Name = "Medishare",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Insurance").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Insurance").FirstOrDefault()
                },
                new Payee {
                    Name = "Village of Bloomdale",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Electric Bill").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Electric Bill").FirstOrDefault()
                },
                new Payee {
                    Name = "FedLoan Servicing",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Student Loans").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Student Loans").FirstOrDefault()
                },
                new Payee {
                    Name = "Progressive Insurance",
                    BeginEffectiveDate = DateTime.Parse("8/1/2017"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Insurance").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Insurance").FirstOrDefault()
                },
                new Payee {
                    Name = "Straight Talk",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Phone").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Phone").FirstOrDefault()
                },
                new Payee {
                    Name = "Brookside",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Tithe").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Tithe").FirstOrDefault()
                },
                new Payee {
                    Name = "Meijer",
                    BeginEffectiveDate = DateTime.Parse("10/1/2016"),
                    BudgetCategoryID = categories.Where(c => c.Name == "Groceries").FirstOrDefault().ID,
                    Category = categories.Where(c => c.Name == "Groceries").FirstOrDefault()
                }
            };

            foreach (var p in payees) {
                context.AddPayee(p);
            }

            // Create Aliases
            List<Alias> aliases = new List<Alias> {
                new Alias {
                    Name = "Dave & Louise",
                    PayeeID = payees.Where(p => p.Name == "Dave and Louise").First().ID
                },
                new Alias {
                    Name = "Louise and Dave",
                    PayeeID = payees.Where(p => p.Name == "Dave and Louise").First().ID
                },
                new Alias {
                    Name = "Louise & Dave",
                    PayeeID = payees.Where(p => p.Name == "Dave and Louise").First().ID
                },
                new Alias {
                    Name = "MEIJER # 051",
                    PayeeID = payees.Where(p => p.Name == "Meijer").First().ID
                },
                new Alias {
                    Name = "MEIJER # 156",
                    PayeeID = payees.Where(p => p.Name == "Meijer").First().ID
                },
                new Alias {
                    Name = "Appraisal Re6366 APP RESEAR",
                    PayeeID = payees.Where(p => p.Name == "Appraisal Research").First().ID
                },
                new Alias {
                    Name = "FEDLOANSERVICING STDNT LOAN",
                    PayeeID = payees.Where(p => p.Name == "FedLoan Servicing").First().ID
                },
                new Alias {
                    Name = "PROGRESSIVE *INSURANCE",
                    PayeeID = payees.Where(p => p.Name == "Progressive Insurance").First().ID
                },
                new Alias {
                    Name = "STRAIGHTTALK*AIRTIME",
                    PayeeID = payees.Where(p => p.Name == "Straight Talk").First().ID
                },
                new Alias {
                    Name = "Brookside Evange WEB PMTS",
                    PayeeID = payees.Where(p => p.Name == "Brookside").First().ID
                }
            };

            foreach (var a in aliases) {
                context.AddAlias(a);
            }

            await context.SaveChangesAsync();
        }
    }
}