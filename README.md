# N.E.X.U.S. Warehouse and Logistics Hub

N.E.X.U.S. is a console-based C# application that simulates warehouse inventory, order fulfilment, worker allocation, vehicle dispatch, and delivery monitoring.

## Running the application

Open `N.E.X.U.S Warehouse and Logistics Hub.csproj` in Visual Studio and run the project, or build it from a Developer Command Prompt:

```powershell
dotnet build "N.E.X.U.S Warehouse and Logistics Hub.csproj"
```

Then start `bin\Debug\N.E.X.U.S Warehouse and Logistics Hub.exe`.

The project targets .NET Framework 4.7.2 and uses Spectre.Console for its console UI.

## Main features

- Create, update, remove, and view inventory items.
- Create orders containing multiple inventory items.
- Allocate the required pickers and drivers for an order, then reserve them until delivery finishes.
- Select a vehicle only when its capacity can carry the full order weight.
- Run periodic inventory and vehicle monitoring in the background.
- Show a dashboard for workers, vehicles, orders, inventory, and recent events.
- Write operational messages to `nexus_log.txt`.

Startup item weights, stock quantities, and perishable expiry windows are randomized for each application run.

## Design decisions

- `InventoryItem` and `Vehicle` are abstract base classes. Perishable, fragile, and bulk items override handling rules; vans and trucks override vehicle type behaviour.
- `IDispatchable`, `IMonitorable`, and `ILogger` keep dispatching, monitoring, and logging responsibilities explicit. `Warehouse` accepts an `ILogger` dependency.
- A single warehouse lock protects inventory, orders, workers, and vehicles while background tasks operate.
- `CapacityExceededException` and `WorkforceShortageException` represent domain failures. Workers are released if dispatch cannot continue or an unexpected processing error occurs.

## Events and background processing

`WarehouseEvents` publishes three custom events:

- `Alert` for low stock, expired stock, and processing failures.
- `OrderDispatched` when a vehicle leaves the warehouse.
- `OrderDelivered` when delivery completes.

`StartMonitoring` runs periodic checks independently of user input. Its cancellation token is cancelled in `Program`'s `finally` block when the application exits. Order picking/dispatch and delivery also run asynchronously, keeping the menu responsive.
