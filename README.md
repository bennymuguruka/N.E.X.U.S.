# N.E.X.U.S. Warehouse and Logistics Hub

N.E.X.U.S. is a console-based Smart Operations Console System for a warehouse and logistics hub. It models inventory, fulfilment, workforce allocation, vehicle capacity, dispatching, delivery, monitoring, and operational alerts in one interactive application.

This project fulfils the PRG2781 requirement for a unique, domain-specific, real-time console system. Its warehouse domain uses randomized start-up data and operational rules so that each run represents a different working scenario.

## How to run

### Requirements

- Windows
- .NET Framework 4.7.2
- NuGet packages restored from `packages.config` (including Spectre.Console)

### Run from Visual Studio

1. Open `N.E.X.U.S Warehouse and Logistics Hub.csproj`.
2. Restore NuGet packages if Visual Studio prompts you to do so.
3. Build and run the project.

### Run from PowerShell

```powershell
dotnet build "N.E.X.U.S Warehouse and Logistics Hub.csproj"
& ".\bin\Debug\N.E.X.U.S Warehouse and Logistics Hub.exe"
```

The application writes its audit log beside the running executable at `bin\Debug\nexus_log.txt`.

## System overview

The application opens with a numbered, menu-driven console interface. The left side provides warehouse actions; the right-side **Alerts & Events** panel keeps the ten newest operational messages. The available actions are:

- View, add, update, and remove inventory items.
- Create orders from available stock.
- Process the next pending order without leaving the menu.
- View order status and the operational dashboard.
- Exit safely, cancelling the background monitor.

Startup data is deliberately randomized: item weights, stock quantities, and the expiry window of perishable inventory differ on each run. This makes capacity, staffing, low-stock, and expiry scenarios dynamic rather than hard-coded.

## Warehouse rules and workflow

An order reserves inventory when it is created. Its total weight determines its staffing requirement:

- One picker is required for every 7 kg, rounded up.
- One driver is required for each order.
- A vehicle can only be assigned when it is available and its capacity can carry the full order weight.

Processing follows this state flow:

```text
Pending -> Picking -> Packed -> Dispatched -> Delivered
```

A pending order reserves its required workers, is picked, packed, assigned to a suitable vehicle, and dispatched. Delivery occurs asynchronously. Once delivery completes, the vehicle is unloaded, all assigned workers are released, and the next pending order is started automatically.

If no suitable vehicle is currently available, the order returns to `Pending` and its workers are released. Unexpected processing failures mark the order as `Failed` and also release its workers.

### Workforce-aware order splitting

N.E.X.U.S. includes a custom feature beyond the standard course requirements: oversized orders are split when the total picker requirement is greater than the number of pickers employed by the warehouse. The system partitions the items into smaller capacity-safe orders, preserves the original ID for the first batch, assigns new IDs to the remaining batches, and queues the batches sequentially.

This prevents an order from remaining permanently blocked because its staffing requirement exceeds the warehouse's maximum workforce. Orders that are valid but temporarily waiting for busy staff remain intact and process when resources are released.

## Object-oriented design

### Encapsulation

Domain objects expose state through properties, including item details, order status, worker availability, and vehicle load. Workflow mutations are coordinated by `Warehouse`, which is responsible for allocation, dispatch, release, and monitoring decisions.

### Abstraction, inheritance, and polymorphism

- `InventoryItem` is an abstract base class. `BulkItem`, `FragileItem`, and `PerishableItem` override `GetHandlingRule()` to provide different handling behaviour. `PerishableItem` also supplies expiry checking.
- `Vehicle` is an abstract base class. `Van` and `Truck` override `GetVehicleType()` and carry different capacities.
- `Worker` is an abstract base class. `Picker` and `Driver` override `GetRole()`.

The UI uses these shared base types and overridden methods, allowing it to display the correct domain-specific behaviour without separate logic for every derived class.

## Interfaces and dependency injection

The system uses three meaningful interfaces:

- `IDispatchable` defines `Dispatch()` for objects that can leave the warehouse. `Vehicle` implements it.
- `IMonitorable` defines `CheckStatus()` for objects monitored by the periodic operational checks. `Warehouse` and `Vehicle` implement it.
- `ILogger` defines `Log(string message)`, allowing `Warehouse` to use an injected logging implementation rather than directly depending on one concrete logger.

`Warehouse` accepts an `ILogger` through its constructor. The default constructor injects `ConsoleLogger`, demonstrating basic dependency injection while retaining a convenient default application setup.

## Exceptions and error handling

The application uses validation and exception handling to keep invalid input and operational failures from crashing the system.

- Menu and entity input is validated before use.
- `CapacityExceededException` is thrown if a vehicle load would exceed capacity.
- `WorkforceShortageException` is thrown when the currently available workers cannot satisfy an order's requirements.
- `Program.Main()` wraps each selected menu action in `try-catch`, displaying a user-friendly error instead of terminating the program.
- `try-finally` ensures the background monitoring task is stopped when the application exits.
- Background processing catches unexpected failures, releases workers, marks the order as failed, and raises an alert.

## Events and delegates

`WarehouseEvents` uses the custom `WarehouseEventHandler` delegate and publishes three domain events:

- `Alert` - low stock, expired items, workforce shortages, processing errors, and workflow warnings.
- `OrderDispatched` - raised when an order leaves using an allocated vehicle.
- `OrderDelivered` - raised when an order reaches its completed state.

`Warehouse` subscribes to these events through `HandleAlert`, `HandleOrderDispatched`, and `HandleOrderDelivered`. The handlers update the live Alerts & Events panel and record the event through the logger. This separates the workflow that raises an event from the UI and logging responses to it.

## Multithreading and safe background work

The system uses `Task.Run`, `async`/`await`, and a cancellation token to simulate concurrent warehouse activity without blocking menu input.

- `StartMonitoring()` runs an independent periodic monitoring task. Every five seconds it checks stock and expiry conditions.
- Picking/packing/dispatching runs asynchronously after an order is started.
- Delivery runs as a separate asynchronous task and releases workers and vehicles after its delay.
- A shared `syncLock` protects inventory, orders, workers, and vehicles whenever foreground and background operations access the same state.
- `CancellationTokenSource` allows the monitoring loop to shut down cleanly on exit.

These controls let the menu remain responsive while orders progress and monitoring continues in the background.

## Bonus features

The project integrates several bonus features in the main workflow:

- **File I/O and logging:** `ConsoleLogger` appends a timestamped audit trail to `nexus_log.txt` beside the executable.
- **LINQ:** order lookup, worker allocation, vehicle selection, totals, and dashboard counts use LINQ queries.
- **Async/await:** fulfilment and delivery simulation are asynchronous.
- **Basic dependency injection:** `ILogger` is supplied to `Warehouse` through its constructor.
- **Custom workflow feature:** workforce-aware order splitting prevents oversized orders from deadlocking the queue.
- **Spectre.Console UI:** formatted inventory tables, dashboard panels, charts, and an event feed improve readability while keeping the application strictly console based.

## Presentation guide

For the demonstration, create an order, select **Process Order**, and use the Alerts & Events panel or Dashboard to show its progression through processing, dispatch, and delivery. Demonstrate a low-stock or expired-item alert, then explain how the monitoring task, custom events, logger, and synchronization lock work together.