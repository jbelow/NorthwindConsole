using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using NLog.Web;
using NorthwindConsole.Model;

/*
link to the project start files from Jeff:
https://github.com/jgrissom/NorthwindConsole


*/


namespace NorthwindConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do
                {
                    Console.WriteLine("1) Display Categories");
                    Console.WriteLine("2) Add a new Category");
                    Console.WriteLine("3) Display Category and related products");
                    Console.WriteLine("4) Display all Categories and their related products");
                    Console.WriteLine("5) Add a new Product");
                    Console.WriteLine("\"q\" to quit");
                    choice = Console.ReadLine();
                    Console.Clear();

                    logger.Info($"Option {choice} selected");
                    //moved var db here because it was needless in every if statment
                    var db = new NorthwindConsole_31_JEBContext();
                    //Display Categories
                    if (choice == "1")
                    {
                        DisplayCategories(db);
                    }
                    //Add a new Category
                    else if (choice == "2")
                    {
                        Categories category = new Categories();
                        Console.WriteLine("Enter Category Name:");
                        category.CategoryName = Console.ReadLine();
                        Console.WriteLine("Enter the Category Description:");
                        category.Description = Console.ReadLine();

                        ValidationContext context = new ValidationContext(category, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        //this is just a bool and gets as asigned on run like in js 
                        var isValid = Validator.TryValidateObject(category, context, results, true);
                        if (isValid)
                        {
                            // check for unique name
                            if (db.Categories.Any(c => c.CategoryName == category.CategoryName))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Name already exists", new string[] { "CategoryName" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                Console.WriteLine(category.CategoryName + " " + category.Description);
                                db.Categories.Add(category);
                                db.SaveChanges();
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    }
                    //Display Category and related products
                    else if (choice == "3")
                    {
                        var query = db.Categories.OrderBy(p => p.CategoryId);

                        Console.WriteLine("Select the category whose products you want to display:");
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryId}) {item.CategoryName}");
                        }
                        Console.ForegroundColor = ConsoleColor.White;
                        int id = int.Parse(Console.ReadLine());
                        Console.Clear();
                        logger.Info($"CategoryId {id} selected");

                        // Categories category = db.Categories.FirstOrDefault(c => c.CategoryId == id);
                        Categories category = db.Categories.Include("Products").FirstOrDefault(c => c.CategoryId == id);
                        Console.WriteLine($"{category.CategoryName} - {category.Description}");

                        foreach (Products p in category.Products)
                        {
                            Console.WriteLine(p.ProductName);
                        }
                    }
                    //Display all Categories and their related products
                    else if (choice == "4")
                    {
                        var query = db.Categories.Include("Products").OrderBy(p => p.CategoryId);
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{item.CategoryName}");
                            foreach (Products p in item.Products)
                            {
                                Console.WriteLine($"\t{p.ProductName}");
                            }
                        }
                    }
                    //Add a new Product
                    else if (choice == "5")
                    {
                        //add a new product
                        Categories category = new Categories();
                        Products product = new Products();
                        DisplayCategories(db);
                        Console.WriteLine("Enter the category ID for this product:");
                        product.CategoryId = Int32.Parse(Console.ReadLine());

                        //this is just a bool and gets as asigned on run like in js 
                        {
                            // check if id is real
                            if (db.Categories.Any(c => c.CategoryId == product.CategoryId))
                            {
                                logger.Info("Category picked successfully!");

                                Console.WriteLine("Enter Product Name:");
                                product.ProductName = Console.ReadLine();

                                if (db.Products.Any(c => c.ProductName != product.ProductName))
                                {

                                }
                                else
                                {
                                logger.Error("Entered Product Name is the same as an  existing Product Name");
                                }
                                Console.WriteLine("Enter the Product quantity per unit:");
                                product.QuantityPerUnit = Console.ReadLine();
                            }
                            else
                            {
                                logger.Error("Entered Categories Id doesn't match any Id in Categories");

                            }
                        }


                    }
                    Console.WriteLine();

                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }

            logger.Info("Program ended");
        }

        //display categories method - so that it can be reused
        private static void DisplayCategories(NorthwindConsole_31_JEBContext db)
        {
            var query = db.Categories.OrderBy(p => p.CategoryId);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{query.Count()} records returned");
            Console.ForegroundColor = ConsoleColor.Magenta;
            foreach (var item in query)
            {
                Console.WriteLine(String.Format("{0,-10} | {1,-10} {2,-10}", $"ID: {item.CategoryId}", $"Name: {item.CategoryName}", $"- {item.Description}"));
            }
            Console.ForegroundColor = ConsoleColor.White;
        }


    }
}