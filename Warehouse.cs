using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
    public class Warehouse : IMonitorable
    {
        private readonly object syncLock = new object();
        private readonly Random random = new Random();
        private readonly HashSet<string> raisedMonitoringAlerts = new HashSet<string>();
        private CancellationTokenSource monitoringCancellation;

        public List<InventoryItem> Items { get; set; }

        public List<Order> Orders { get; set; }

        public List<Worker> Workers { get; set; }

        public List<Vehicle> Vehicles { get; set; }

        private ILogger logger;

        private WarehouseEvents events;

        public Warehouse()
    : this(new ConsoleLogger())
        {

        }

        public Warehouse(ILogger logger)
        {
            if (logger is ConsoleLogger cl)
            {
                cl.Logged += entry => Console.Title = $"N.E.X.U.S. — {entry}";
            }

            Items = new List<InventoryItem>();

            Orders = new List<Order>();

            Workers = new List<Worker>();

            Vehicles = new List<Vehicle>();

            this.logger = logger;

            events = new WarehouseEvents();

            events.Alert += HandleAlert;
            events.OrderDispatched += HandleOrderDispatched;
            events.OrderDelivered += HandleOrderDelivered;

            CreateData();
        }


        private void CreateData()
        {
            // Items

            Items.Add(
                new BulkItem(
                    1,
                    "Rice",
                    random.Next(3, 8),
                    random.Next(8, 21),
                    "A1"));

            Items.Add(
                new PerishableItem(
                    2,
                    "Milk",
                    random.Next(1, 4),
                    random.Next(8, 21),
                    "A2",
                    DateTime.Now.AddMinutes(random.Next(5, 31))));

            Items.Add(
                new FragileItem(
                    3,
                    "Glass",
                    random.Next(2, 6),
                    random.Next(4, 16),
                    "A3"));


            // Workers

            Workers.Add(
                new Picker(1, "Picker 1"));

            Workers.Add(
                new Picker(2, "Picker 2"));

            Workers.Add(
                new Picker(3, "Picker 3"));

            Workers.Add(
                new Picker(4, "Picker 4"));

            Workers.Add(
                new Driver(5, "Driver 1"));

            Workers.Add(
                new Driver(6, "Driver 2"));


            // Vehicles

            Vehicles.Add(
                new Van("V1"));

            Vehicles.Add(
                new Truck("T1"));

            logger.Log(
                "N.E.X.U.S. started.");
        }

        public void UpdateItem()
        {
            Console.Write("\nEnter Item ID to update: ");
            int id;
            if (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid Item ID.");
                return;
            }

            lock (syncLock)
            {
                InventoryItem item = Items.FirstOrDefault(x => x.Id == id);
                if (item == null)
                {
                    Console.WriteLine("Item not found.");
                    return;
                }

                Console.Write($"Name ({item.Name}, press Enter to keep): ");
                string name = Console.ReadLine();
                string updatedName = string.IsNullOrWhiteSpace(name) ? item.Name : name;

                Console.Write($"Weight ({item.Weight}kg, press Enter to keep): ");
                string weightInput = Console.ReadLine();
                double updatedWeight = item.Weight;
                if (!string.IsNullOrWhiteSpace(weightInput))
                {
                    double weight;
                    if (!double.TryParse(weightInput, out weight) || weight <= 0)
                    {
                        Console.WriteLine("Invalid weight. Item was not changed.");
                        return;
                    }
                    updatedWeight = weight;
                }

                Console.Write($"Zone ({item.Zone}, press Enter to keep): ");
                string zone = Console.ReadLine();
                string updatedZone = string.IsNullOrWhiteSpace(zone) ? item.Zone : zone;

                item.Name = updatedName;
                item.Weight = updatedWeight;
                item.Zone = updatedZone;

                logger.Log($"Item #{item.Id} updated.");
                Console.WriteLine("Item updated.");
            }
        }

        public void RemoveItem()
        {
            Console.Write("\nEnter Item ID to remove: ");
            int id;
            if (!int.TryParse(Console.ReadLine(), out id))
            {
                Console.WriteLine("Invalid Item ID.");
                return;
            }

            lock (syncLock)
            {
                InventoryItem item = Items.FirstOrDefault(x => x.Id == id);
                if (item == null)
                {
                    Console.WriteLine("Item not found.");
                    return;
                }

                Console.Write($"Remove {item.Name}? Type YES to confirm: ");
                if (!string.Equals(Console.ReadLine(), "YES", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Removal cancelled.");
                    return;
                }

                Items.Remove(item);
                logger.Log($"Item #{id} ({item.Name}) removed.");
                Console.WriteLine("Item removed.");
            }
        }


        // =================================
        // DISPLAY ITEMS
        // =================================

        public void ShowItems()
        {
            Console.WriteLine(
                "\n===== INVENTORY =====");

            foreach (InventoryItem item in Items)
            {
                Console.WriteLine(
                    $"{item.Id}. " +
                    $"{item.Name} | " +
                    $"{item.Quantity} units | " +
                    $"{item.Weight}kg | " +
                    $"Zone {item.Zone}");
            }
        }

        // =================================
        // ADD ITEM
        // =================================

        public void AddItem()
        {
            Console.Write("\nEnter Item ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Item ID. Please enter a whole number.");
                return;
            }

            lock (syncLock)
            {
                InventoryItem existing = Items.FirstOrDefault(x => x.Id == id);

                if (existing != null)
                {
                    Console.Write($"Item #{id} ({existing.Name}) already exists. Enter quantity to add: ");
                    if (!int.TryParse(Console.ReadLine(), out int addQty) || addQty <= 0)
                    {
                        Console.WriteLine("Invalid quantity. Please enter a whole number greater than 0.");
                        return;
                    }

                    existing.Quantity += addQty;

                    logger.Log($"Item #{id} ({existing.Name}) quantity increased by {addQty}. New total: {existing.Quantity}.");
                    Console.WriteLine($"\nUpdated {existing.Name}: {existing.Quantity} units.");
                    return;
                }

                Console.Write("Enter item name: ");
                string name = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(name))
                {
                    Console.WriteLine("Item name cannot be empty.");
                    return;
                }

                Console.Write("Enter weight (kg): ");
                if (!double.TryParse(Console.ReadLine(), out double weight) || weight <= 0)
                {
                    Console.WriteLine("Invalid weight. Please enter a number greater than 0.");
                    return;
                }

                Console.Write("Enter quantity: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Invalid quantity. Please enter a whole number greater than 0.");
                    return;
                }

                Console.Write("Enter zone (e.g. A1): ");
                string zone = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(zone))
                {
                    Console.WriteLine("Zone cannot be empty.");
                    return;
                }

                Console.WriteLine("Item type: 1) Bulk  2) Perishable  3) Fragile");
                Console.Write("Choose type: ");
                string typeChoice = Console.ReadLine();

                InventoryItem newItem;

                switch (typeChoice)
                {
                    case "2":
                        Console.Write("Enter expiry date (yyyy-MM-dd): ");
                        if (!DateTime.TryParse(Console.ReadLine(), out DateTime expiry))
                        {
                            Console.WriteLine("Invalid date. Item not added.");
                            return;
                        }
                        newItem = new PerishableItem(id, name, weight, quantity, zone, expiry);
                        break;

                    case "3":
                        newItem = new FragileItem(id, name, weight, quantity, zone);
                        break;

                    default:
                        newItem = new BulkItem(id, name, weight, quantity, zone);
                        break;
                }

                Items.Add(newItem);

                logger.Log($"Item #{id} ({newItem.Name}) added: {quantity} units, {weight}kg, Zone {zone}.");
                Console.WriteLine($"\nItem #{id} ({newItem.Name}) added.");
            }
        }
        // =================================
        // CREATE ORDER
        // =================================

        public void CreateOrder()
        {
            Console.Write("\nEnter Order ID: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("Invalid Order ID. Please enter a whole number.");
                return;
            }

            if (Orders.Any(o => o.Id == id))
            {
                Console.WriteLine($"Order #{id} already exists. Choose a different ID.");
                return;
            }

            var requestedItems = new Dictionary<int, int>();
            while (true)
            {
                Console.Write("Enter Item ID (press Enter when finished): ");
                string itemInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(itemInput)) break;

                int itemId;
                if (!int.TryParse(itemInput, out itemId))
                {
                    Console.WriteLine("Invalid Item ID. Please enter a whole number.");
                    continue;
                }

                Console.Write("Enter quantity: ");
                int quantity;
                if (!int.TryParse(Console.ReadLine(), out quantity) || quantity <= 0)
                {
                    Console.WriteLine("Invalid quantity. Please enter a whole number greater than 0.");
                    continue;
                }

                requestedItems[itemId] = requestedItems.ContainsKey(itemId)
                    ? requestedItems[itemId] + quantity : quantity;
            }

            if (requestedItems.Count == 0)
            {
                Console.WriteLine("An order must contain at least one item.");
                return;
            }

            Order order;
            lock (syncLock)
            {
                foreach (var request in requestedItems)
                {
                    InventoryItem item = Items.FirstOrDefault(x => x.Id == request.Key);
                    if (item == null || request.Value > item.Quantity)
                    {
                        string reason = item == null ? "not found" : "insufficient stock";
                        Console.WriteLine(item == null ? "Item not found." : "Not enough stock.");
                        logger.Log($"Order creation failed: Item #{request.Key} {reason}.");
                        return;
                    }
                }

                var orderItems = new List<InventoryItem>();
                foreach (var request in requestedItems)
                {
                    InventoryItem item = Items.First(x => x.Id == request.Key);
                    orderItems.Add(CreateOrderItem(item, request.Value));
                    item.Quantity -= request.Value;
                }

                order = new Order(id, orderItems);
                Orders.Add(order);
            }


            logger.Log(
                $"Order #{id} created.");

            Console.WriteLine(
                $"\nOrder #{id} created.");

            Console.WriteLine(
                $"Weight: " +
                $"{order.GetTotalWeight()}kg");

            Console.WriteLine(
                $"Pickers needed: " +
                $"{order.PickersNeeded}");
        }


        // =================================
        // PROCESS ORDER
        // =================================

        public void ProcessOrders()
        {
            Order order;

            lock (syncLock)
            {
                order = GetNextPendingOrder();
                if (order == null)
                {
                    Console.WriteLine("No pending orders.");
                    return;
                }

                try
                {
                    AllocateWorkers(order);
                }
                catch (WorkforceShortageException exception)
                {
                    events.RaiseAlert(exception.Message);
                    logger.Log(exception.Message);
                    return;
                }

                StartPicking(order);
            }

            CompletePickingAndDispatchAsync(order);
            Console.WriteLine($"Order #{order.Id} is being processed in the background.");
        }

        private void CompletePickingAndDispatchAsync(Order order)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                Vehicle vehicle = null;
                try
                {
                    lock (syncLock)
                    {
                        PackOrder(order);
                        vehicle = FindSuitableVehicle(order);
                        if (vehicle == null)
                        {
                            ReleaseWorkers(order);
                            order.Status = OrderStatus.Pending;
                        }
                        else
                        {
                            DispatchOrder(order, vehicle);
                        }
                    }

                    if (vehicle == null)
                    {
                        events.RaiseAlert($"No suitable vehicle available for Order #{order.Id}.");
                        return;
                    }

                    events.RaiseOrderDispatched($"Order #{order.Id} departed using {vehicle.Id}.");
                    CompleteDeliveryAsync(order, vehicle);
                }
                catch (Exception exception)
                {
                    lock (syncLock)
                    {
                        ReleaseWorkers(order);
                        order.Status = OrderStatus.Failed;
                    }

                    events.RaiseAlert($"Order #{order.Id} processing failed: {exception.Message}");
                    logger.Log($"Order #{order.Id} processing failed: {exception.Message}");
                }
            });
        }

        private InventoryItem CreateOrderItem(InventoryItem item, int quantity)
        {
            PerishableItem perishable = item as PerishableItem;
            if (perishable != null)
                return new PerishableItem(perishable.Id, perishable.Name, perishable.Weight, quantity, perishable.Zone, perishable.ExpiryDate);
            if (item is FragileItem)
                return new FragileItem(item.Id, item.Name, item.Weight, quantity, item.Zone);
            return new BulkItem(item.Id, item.Name, item.Weight, quantity, item.Zone);
        }

        private Order GetNextPendingOrder()
        {
            return Orders.FirstOrDefault(x => x.Status == OrderStatus.Pending);
        }

        private void AllocateWorkers(Order order)
        {
            var pickers = Workers.Where(x => x is Picker && x.Available).Take(order.PickersNeeded).ToList();
            var drivers = Workers.Where(x => x is Driver && x.Available).Take(order.DriversNeeded).ToList();
            if (pickers.Count < order.PickersNeeded || drivers.Count < order.DriversNeeded)
                throw new WorkforceShortageException($"Workforce shortage for Order #{order.Id}: requires {order.PickersNeeded} picker(s) and {order.DriversNeeded} driver(s).");

            order.AssignedWorkers = pickers.Concat(drivers).ToList();
            foreach (Worker worker in order.AssignedWorkers)
                worker.Available = false;
        }

        private void ReleaseWorkers(Order order)
        {
            foreach (Worker worker in order.AssignedWorkers)
                worker.Available = true;
            order.AssignedWorkers.Clear();
        }

        private void StartPicking(Order order)
        {
            order.Status = OrderStatus.Picking;
            logger.Log($"Order #{order.Id} is being picked.");
        }

        private void PackOrder(Order order)
        {
            order.Status = OrderStatus.Packed;
            logger.Log($"Order #{order.Id} packed.");
        }

        private Vehicle FindSuitableVehicle(Order order)
        {
            return Vehicles.FirstOrDefault(x => x.Available && x.Capacity >= order.GetTotalWeight());
        }

        private void DispatchOrder(Order order, Vehicle vehicle)
        {
            vehicle.Load(order.GetTotalWeight());
            vehicle.Dispatch();
            order.VehicleId = vehicle.Id;
            order.Status = OrderStatus.Dispatched;
        }

        private void CompleteDeliveryAsync(Order order, Vehicle vehicle)
        {
            Task.Run(async () =>
            {
                await Task.Delay(5000);
                lock (syncLock)
                {
                    order.Status = OrderStatus.Delivered;
                    vehicle.Available = true;
                    vehicle.CurrentLoad = 0;
                    ReleaseWorkers(order);
                }

                events.RaiseOrderDelivered($"Order #{order.Id} delivered.");

                // A completed delivery frees the vehicle and workers reserved for
                // this order. Start the next queued order now that resources may
                // be available instead of requiring another menu action.
                ProcessOrders();
            });
        }


        // =================================
        // MONITOR
        // =================================

        public void StartMonitoring()
        {
            if (monitoringCancellation != null) return;

            monitoringCancellation = new CancellationTokenSource();
            CancellationToken token = monitoringCancellation.Token;
            Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        lock (syncLock)
                        {
                            CheckStatus();
                        }
                        await Task.Delay(5000, token);
                    }
                }
                catch (TaskCanceledException)
                {
                    // Normal shutdown path.
                }
            });
        }

        public void StopMonitoring()
        {
            if (monitoringCancellation == null) return;

            monitoringCancellation.Cancel();
            monitoringCancellation.Dispose();
            monitoringCancellation = null;
        }
        public void CheckStatus()
        {
            CheckItems();
            CheckVehicles();
        }


        private void CheckItems()
        {
            foreach (InventoryItem item in Items)
            {
                if (item is PerishableItem)
                {
                    PerishableItem p =
                        (PerishableItem)item;

                    if (p.IsExpired())
                    {
                        RaiseMonitoringAlertOnce($"expired-{p.Id}", $"Item {p.Name} has expired.");
                    }
                }


                if (item.Quantity <= 2)
                {
                    RaiseMonitoringAlertOnce($"low-stock-{item.Id}", $"Low stock: {item.Name}");
                }
            }
        }


        private void CheckVehicles()
        {
            foreach (Vehicle vehicle in Vehicles)
            {
                vehicle.CheckStatus();
            }
        }


        private void HandleAlert(string message)
        {
            MonitorLog.Add($"[yellow][[EVENT]] {message}[/]");
            logger.Log($"EVENT: {message}");
        }

        private void HandleOrderDispatched(string message)
        {
            MonitorLog.Add($"[blue][[DISPATCHED]] {message}[/]");
            logger.Log(message);
        }

        private void HandleOrderDelivered(string message)
        {
            MonitorLog.Add($"[green][[DELIVERED]] {message}[/]");
            logger.Log(message);
        }

        private void RaiseMonitoringAlertOnce(string key, string message)
        {
            if (raisedMonitoringAlerts.Add(key))
                events.RaiseAlert(message);
        }

    }
}
