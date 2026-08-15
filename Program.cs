using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PRG_281_Project
{
    class Program
    {
        static void Main()
        {
            Warehouse warehouse = new Warehouse();

            warehouse.StartMonitoring();


            bool running = true;


            while (running)
            {
                Console.Clear();

                Console.WriteLine(
                    "================================");

                Console.WriteLine(
                    "       N.E.X.U.S.");

                Console.WriteLine(
                    " Warehouse & Logistics System");

                Console.WriteLine(
                    "================================");


                Console.WriteLine(
                    "\n1. View Inventory");

                Console.WriteLine(
                    "2. Create Order");

                Console.WriteLine(
                    "3. Process Order");

                Console.WriteLine(
                    "4. View Orders");

                Console.WriteLine(
                    "5. Exit");


                Console.Write(
                    "\nChoose: ");

                string choice =
                    Console.ReadLine();


                try
                {
                    switch (choice)
                    {
                        case "1":

                            warehouse.ShowItems();

                            break;


                        case "2":

                            warehouse.CreateOrder();

                            break;


                        case "3":

                            warehouse.ProcessOrders();

                            break;


                        case "4":

                            foreach (Order order
                                     in warehouse.Orders)
                            {
                                Console.WriteLine(
                                    $"Order #{order.Id} - " +
                                    $"{order.Status} - " +
                                    $"Vehicle: " +
                                    $"{order.VehicleId}");
                            }

                            break;


                        case "5":

                            running = false;

                            break;


                        default:

                            Console.WriteLine(
                                "Invalid option.");

                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(
                        $"ERROR: {ex.Message}");
                }


                if (running)
                {
                    Console.WriteLine(
                        "\nPress ENTER...");

                    Console.ReadLine();
                }
            }
        }
    }
}