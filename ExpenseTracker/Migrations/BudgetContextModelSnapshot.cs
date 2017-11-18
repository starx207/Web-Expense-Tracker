﻿// <auto-generated />
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;

namespace ExpenseTracker.Migrations
{
    [DbContext(typeof(BudgetContext))]
    partial class BudgetContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.0.0-rtm-26452");

            modelBuilder.Entity("ExpenseTracker.Models.BudgetCategory", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("Amount");

                    b.Property<DateTime>("BeginEffectiveDate");

                    b.Property<DateTime?>("EndEffectiveDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<int>("Type");

                    b.HasKey("ID");

                    b.ToTable("BudgetCategories");
                });

            modelBuilder.Entity("ExpenseTracker.Models.Payee", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("BeginEffectiveDate");

                    b.Property<int?>("BudgetCategoryID");

                    b.Property<DateTime?>("EndEffectiveDate");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("ID");

                    b.HasIndex("BudgetCategoryID");

                    b.ToTable("Payees");
                });

            modelBuilder.Entity("ExpenseTracker.Models.Transaction", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("Date");

                    b.Property<int?>("OverrideCategoryID");

                    b.Property<int?>("PayeeID");

                    b.HasKey("ID");

                    b.HasIndex("OverrideCategoryID");

                    b.HasIndex("PayeeID");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("ExpenseTracker.Models.Payee", b =>
                {
                    b.HasOne("ExpenseTracker.Models.BudgetCategory", "Category")
                        .WithMany("Payees")
                        .HasForeignKey("BudgetCategoryID");
                });

            modelBuilder.Entity("ExpenseTracker.Models.Transaction", b =>
                {
                    b.HasOne("ExpenseTracker.Models.BudgetCategory", "OverrideCategory")
                        .WithMany()
                        .HasForeignKey("OverrideCategoryID");

                    b.HasOne("ExpenseTracker.Models.Payee", "PayableTo")
                        .WithMany("Transactions")
                        .HasForeignKey("PayeeID");
                });
#pragma warning restore 612, 618
        }
    }
}
