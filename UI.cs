using Spectre.Console;
using System;
using System.Dynamic;
using System.Linq; 

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
    public static class UI
    {
        public enum MenuChoice
        {
            ViewInventory = 1,
            AddItem,
            UpdateItem,
            RemoveItem,
            CreateOrder,
            ProcessOrder,
            ViewOrders,
            Dashboard,
            Exit
        }

        public static MenuChoice ShowMainMenu()
        {
            AnsiConsole.Clear();

            AnsiConsole.Write(
                new FigletText("N.E.X.U.S.").Centered());

            AnsiConsole.MarkupLine("[grey]Warehouse & Logistics System[/]\n");

            var menuOptions = new Markup(
                "[bold cyan]1.[/] View Inventory\n" +
                "[bold cyan]2.[/] Add Item\n" +
                "[bold cyan]3.[/] Update Item\n" +
                "[bold cyan]4.[/] Remove Item\n" +
                "[bold cyan]5.[/] Create Order\n" +
                "[bold cyan]6.[/] Process Order\n" +
                "[bold cyan]7.[/] View Orders\n" +
                "[bold cyan]8.[/] Dashboard\n" +
                "[bold cyan]9.[/] Exit");

            var recentEvents = MonitorLog.GetRecent();
            string eventContent = recentEvents.Count == 0
                ? "[grey]No alerts or events yet.[/]"
                : string.Join("\n", recentEvents);

            var eventPanel = new Panel(new Markup(eventContent))
                .Header("[bold yellow]Alerts & Events[/]")
                .BorderColor(Color.Yellow)
                .Expand();

            AnsiConsole.Write(new Columns(menuOptions, eventPanel));
            AnsiConsole.WriteLine();

            int choice = AnsiConsole.Prompt(
                new TextPrompt<int>("[bold]Choose an option (1-9):[/]")
                    .Validate(value => value >= (int)MenuChoice.ViewInventory && value <= (int)MenuChoice.Exit
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red]Enter a number from 1 to 9.[/]")));

            AnsiConsole.Clear();
            return (MenuChoice)choice;

        }

        public static void ShowInventory(Warehouse warehouse)
        {
            AnsiConsole.Write(new Rule("[bold cyan]N.E.X.U.S.[/]").Centered());
            var table = new Table().Expand();
            table.Title("[bold]Inventory[/]");
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Qty");
            table.AddColumn("Weight (kg)");
            table.AddColumn("Zone");
            table.AddColumn("Handling");

            foreach (var item in warehouse.Items)
            {
                string qtyDisplay = item.Quantity <= 2? $"[red]{item.Quantity}[/]": item.Quantity.ToString();

                table.AddRow(
                    item.Id.ToString(),
                    item.Name,
                    qtyDisplay,
                    item.Weight.ToString(),
                    item.Zone,
                    item.GetHandlingRule());
            }

            AnsiConsole.Write(table);
        }

        public static void ShowOrders(Warehouse warehouse)
        {
            AnsiConsole.Write(new Rule("[bold cyan]N.E.X.U.S.[/]").Centered());
            var table = new Table().Expand();
            table.Title("[bold]Order[/]");
            table.AddColumn("ID");
            table.AddColumn("Status");
            table.AddColumn("Vehicle");
            table.AddColumn("Weight (kg)");

            foreach (var order in warehouse.Orders)
            {
                table.AddRow(order.Id.ToString(),FormatOrderStatus(order.Status),order.VehicleId ?? "-",order.GetTotalWeight().ToString());
            }

            AnsiConsole.Write(table);

        }

        public static void ShowDashboard(Warehouse warehouse)
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold cyan]N.E.X.U.S. Dashboard[/]").Centered());

            var workforcePanel = new Panel(BuildWorkerTable(warehouse))
                .Header("[bold]Workforce[/]")
                .BorderColor(Color.Cyan1);

            var utilizationPanel = new Panel(BuildUtilizationChart(warehouse))
                .Header("[bold]Worker Utilization[/]")
                .BorderColor(Color.Cyan1);

            var vehiclePanel = new Panel(BuildVehicleTable(warehouse))
                .Header("[bold]Vehicles[/]")
                .BorderColor(Color.Blue);

            var orderPanel = new Panel(BuildOrderTable(warehouse))
                .Header("[bold]Orders[/]")
                .BorderColor(Color.Yellow);

            var inventoryPanel = new Panel(BuildInventoryTable(warehouse))
                .Header("[bold]Inventory[/]")
                .BorderColor(Color.Green);

            var logPanel = new Panel(string.Join("\n", MonitorLog.GetRecent()))
                .Header("[bold]Monitor Log[/]")
                .BorderColor(Color.Grey);

            // Top row: workforce + utilization chart side by side
            AnsiConsole.Write(new Columns(workforcePanel, utilizationPanel));

            // Middle row: vehicles + orders side by side
            AnsiConsole.Write(new Columns(vehiclePanel, orderPanel));

            // Bottom row: inventory + monitor log side by side
            AnsiConsole.Write(new Columns(inventoryPanel, logPanel));
        }

        private static BarChart BuildUtilizationChart(Warehouse warehouse)
        {
            int availablePickers = warehouse.Workers.Count(w => w is Picker && w.Available);
            int busyPickers = warehouse.Workers.Count(w => w is Picker && !w.Available);
            int availableDrivers = warehouse.Workers.Count(w => w is Driver && w.Available);
            int busyDrivers = warehouse.Workers.Count(w => w is Driver && !w.Available);

            var chart = new BarChart()
                .Width(60)
                .Label("[bold]Available vs Busy[/]")
                .CenterLabel();

            chart.AddItem("Pickers Available", availablePickers, Color.Green);
            chart.AddItem("Pickers Busy", busyPickers, Color.Yellow);
            chart.AddItem("Drivers Available", availableDrivers, Color.Green);
            chart.AddItem("Drivers Busy", busyDrivers, Color.Yellow);

            return chart;
        }

        private static Table BuildWorkerTable(Warehouse warehouse)
        {
            var table = new Table().Expand();
            table.Title("[bold]Workforce[/]");
            table.AddColumn("ID");
            table.AddColumn("Name");
            table.AddColumn("Role");
            table.AddColumn("Status");

            foreach (var worker in warehouse.Workers)
            {
                string status = worker.Available
                    ? "[green]Available[/]"
                    : "[yellow]Busy[/]";

                table.AddRow(
                    worker.Id.ToString(),
                    worker.Name,
                    worker.GetRole(),
                    status);
            }

            int freePickers = warehouse.Workers.Count(w => w is Picker && w.Available);
            int freeDrivers = warehouse.Workers.Count(w => w is Driver && w.Available);
            table.Caption(
                $"[grey]Free pickers: {freePickers} | Free drivers: {freeDrivers}[/]");

            return table;
        }

        private static Table BuildVehicleTable(Warehouse warehouse)
        {
            var table = new Table().Expand();
            table.Title("[bold]Vehicles[/]");
            table.AddColumn("ID");
            table.AddColumn("Type");
            table.AddColumn("Load / Capacity (kg)");
            table.AddColumn("Status");

            foreach (var vehicle in warehouse.Vehicles)
            {
                string status = vehicle.Available
                    ? "[green]Available[/]"
                    : "[yellow]Dispatched[/]";

                table.AddRow(
                    vehicle.Id,
                    vehicle.GetVehicleType(),
                    $"{vehicle.CurrentLoad}/{vehicle.Capacity}",
                    status);
            }

            return table;
        }

        private static Table BuildOrderTable(Warehouse warehouse)
        {
            var table = new Table().Expand();
            table.Title("[bold]Orders[/]");
            table.AddColumn("ID");
            table.AddColumn("Status");
            table.AddColumn("Vehicle");

            foreach (var order in warehouse.Orders)
            {
                table.AddRow(
                    order.Id.ToString(),
                    FormatOrderStatus(order.Status),
                    order.VehicleId ?? "-");
            }

            return table;
        }

        private static Table BuildInventoryTable(Warehouse warehouse)
        {
            var table = new Table().Expand();
            table.Title("[bold]Inventory[/]");
            table.AddColumn("Name");
            table.AddColumn("Qty");
            table.AddColumn("Zone");

            foreach (var item in warehouse.Items)
            {
                string qty = item.Quantity <= 2
                    ? $"[red]{item.Quantity}[/]"
                    : item.Quantity.ToString();

                table.AddRow(item.Name, qty, item.Zone);
            }

            return table;
        }

        private static string FormatOrderStatus(OrderStatus status)
        {
            switch (status)
            {
                case OrderStatus.Pending:
                    return "[grey]Pending[/]";
                case OrderStatus.Picking:
                    return "[yellow]Picking[/]";
                case OrderStatus.Packed:
                    return "[yellow]Packed[/]";
                case OrderStatus.Dispatched:
                    return "[blue]Dispatched[/]";
                case OrderStatus.Delivered:
                    return "[green]Delivered[/]";
                case OrderStatus.Failed:
                    return "[red]Failed[/]";
                default:
                    return status.ToString();
            }
        }

        public static void Pause()
        {
            AnsiConsole.MarkupLine("\n[grey]Press ENTER to continue...[/]");
            Console.ReadLine();
        }

        public static void ShowError(string message)
        {
            AnsiConsole.MarkupLine($"[red]ERROR:[/] {message}");
        }


    }
}
