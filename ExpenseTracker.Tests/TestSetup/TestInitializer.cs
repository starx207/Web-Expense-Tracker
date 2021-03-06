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
                    EffectiveFrom = new DateTime(2015, 02, 01),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 2,
                    Name = "Groceries",
                    Amount = 750,
                    EffectiveFrom = new DateTime(2017, 10, 01),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 3,
                    Name = "Old Groceries",
                    Amount = 700,
                    EffectiveFrom = new DateTime(2016, 10, 01),
                    Type = BudgetType.Expense
                },
                new BudgetCategory {
                    ID = 4,
                    Name = "Income",
                    Amount = 0,
                    EffectiveFrom = new DateTime(2016, 06, 27),
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
                    EffectiveFrom = new DateTime(2016, 7, 1),
                    BudgetCategoryID = 4,
                    Category = categories.Where(c => c.ID == 4).FirstOrDefault()
                },
                new Payee {
                    ID = 2,
                    Name = "Drum Lessons",
                    EffectiveFrom = new DateTime(2017, 4, 1),
                    BudgetCategoryID = 4,
                    Category = categories.Where(c => c.ID == 4).FirstOrDefault()
                },
                new Payee {
                    ID = 3,
                    Name = "Kroger",
                    EffectiveFrom = new DateTime(2014, 6, 14),
                    BudgetCategoryID = 2,
                    Category = categories.Where(c => c.ID == 2).FirstOrDefault()
                },
                new Payee {
                    ID = 4,
                    Name = "Wal Mart",
                    EffectiveFrom = new DateTime(2014, 8, 1),
                    BudgetCategoryID = 3,
                    Category = categories.Where(c => c.ID == 3).FirstOrDefault()
                },
                new Payee {
                    ID = 5,
                    Name = "Money Pit",
                    EffectiveFrom = new DateTime(2017, 1, 1),
                    BudgetCategoryID = null,
                    Category = null
                },
                new Payee {
                    ID = 6,
                    Name = "Payee with Aliases",
                    EffectiveFrom = new DateTime(2017, 1, 1),
                    BudgetCategoryID = 2,
                    Category = categories.Where(c => c.ID == 2).FirstOrDefault()
                }
            };

            return payees;
        }

        public static List<Alias> CreateTestAliases(IQueryable<Payee> payees) {
            Payee payeeWithAliases = payees.Where(p => p.Name == "Payee with Aliases").FirstOrDefault();
            List<Alias> aliases = new List<Alias> {
                new Alias {
                    ID = 1,
                    Name = "WalMart 223235 WAL",
                    PayeeID = payeeWithAliases.ID,
                    AliasForPayee = payeeWithAliases
                },
                new Alias {
                    ID = 2,
                    Name = "WalMart 37917",
                    PayeeID = payeeWithAliases.ID,
                    AliasForPayee = payeeWithAliases
                },
                new Alias {
                    ID = 3,
                    Name = "Murphy USA",
                    PayeeID = payeeWithAliases.ID,
                    AliasForPayee = payeeWithAliases
                }
            };

            return aliases;
        }

        public static List<Transaction> CreateTestTransactions(IQueryable<BudgetCategory> categories, IQueryable<Payee> payees) {
            List<Transaction> transactions = new List<Transaction> {
                new Transaction {
                    ID = 1,
                    Date = DateTime.Parse("7/15/2017"),
                    Amount = 2000,
                    PayeeID = 1,
                    PayableTo = payees.Where(p => p.ID == 1).First()
                },
                new Transaction {
                    ID = 2,
                    Date = DateTime.Parse("10/06/2017"),
                    PayeeID = 3,
                    PayableTo = payees.Where(p => p.ID == 3).First(),
                    Amount = 150.43
                },
                new Transaction {
                    ID = 3,
                    Date = DateTime.Parse("11/1/2017"),
                    Amount = 30
                },
                new Transaction {
                    ID = 4,
                    Date = DateTime.Parse("6/12/2017"),
                    PayeeID = 3,
                    PayableTo = payees.Where(p => p.ID == 3).First(),
                    Amount = 148.04,
                    OverrideCategoryID = 3,
                    OverrideCategory = categories.Where(c => c.ID == 3).First()
                }
            };

            return transactions;
        }
    }
}