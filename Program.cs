using N.E.X.U.S_Warehouse_and_Logistics_Hub;
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
                var choice = UI.ShowMainMenu();
                try
                {
                    switch (choice)
                    {
                        case UI.MenuChoice.ViewInventory:
                            UI.ShowInventory(warehouse);
                            break;

                        case UI.MenuChoice.CreateOrder:
                            warehouse.CreateOrder();
                            break;

                        case UI.MenuChoice.ProcessOrder:
                            warehouse.ProcessOrders();
                            break;

                        case UI.MenuChoice.ViewOrders:
                            UI.ShowOrders(warehouse);
                            break;

                        case UI.MenuChoice.Dashboard:
                            UI.ShowDashboard(warehouse);
                            break;

                        case UI.MenuChoice.Exit:
                            running = false;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    UI.ShowError(ex.Message);
                }
                if (running)
                {
                    UI.Pause();
                }
            }
        }
    }
}