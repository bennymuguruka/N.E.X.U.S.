# N.E.X.U.S. Warehouse and Logistics Hub

N.E.X.U.S. is a .NET Framework 4.7.2 console application that models a small but complete warehouse fulfilment operation. It combines inventory control, order composition, workforce planning, vehicle dispatch, asynchronous delivery, monitoring, and an operator-facing Spectre.Console interface.

## Run the application

Open `N.E.X.U.S Warehouse and Logistics Hub.csproj` in Visual Studio and run it, or build it from PowerShell:

```powershell
dotnet build "N.E.X.U.S Warehouse and Logistics Hub.csproj"
& ".\bin\Debug\N.E.X.U.S Warehouse and Logistics Hub.exe"
```

The start-up catalogue uses randomized stock quantities, weights, and perishable expiry windows so each run represents a fresh operating scenario.

## Operator experience

The main screen presents numbered warehouse actions beside an **Alerts & Events** panel. The panel keeps the ten most recent operational events, including processing, dispatch, delivery, low-stock, expiry, and workflow alerts. The dashboard provides a richer operational view of inventory, orders, workers, vehicle capacity, and recent activity.

Choose **Process Order** to begin the next pending order without leaving the menu. The workflow continues in the background, allowing the operator to inspect stock, create new orders, or view the dashboard while picking and delivery progress.

## Fulfilment workflow

1. An operator creates an order from available inventory. Requested stock is validated and reserved immediately.
2. The warehouse calculates the required workforce: one picker per 7 kg, plus one driver.
3. Available workers are reserved and the order enters the `Picking` state.
4. After picking, the system selects an available vehicle whose capacity can carry the entire order.
5. The vehicle dispatches, then a background delivery task marks the order as delivered, releases the vehicle, and returns workers to the available pool.
6. The next pending order starts automatically when the resources become available.

### Workforce-aware order splitting

An order that needs more pickers than the warehouse employs is not left permanently blocked. N.E.X.U.S. partitions its item quantities into capacity-safe child orders, retaining the original order ID for the first batch and generating IDs for the remaining batches. The batches require no more pickers than the warehouse has, are inserted into the pending queue in sequence, and are processed one after another as workers and vehicles are released.

Orders that can be handled by the full workforce but are temporarily waiting for busy workers are kept intact in the queue. This avoids needless splitting while preserving FIFO-style processing.

## Architecture and safeguards

- `InventoryItem` and `Vehicle` are abstract base classes. Bulk, fragile, and perishable stock implement their own handling requirements; vans and trucks expose their own type and capacity.
- `IDispatchable`, `IMonitorable`, and `ILogger` separate dispatching, monitoring, and persistence concerns. `Warehouse` accepts an `ILogger` dependency.
- A shared synchronization lock protects orders, inventory, staff, and vehicles across menu actions, monitoring, and background tasks.
- `WarehouseEvents` publishes alerts, dispatches, and deliveries. Event handlers update the menu event feed and durable `nexus_log.txt` audit log.
- `CapacityExceededException` and `WorkforceShortageException` model domain failures. Exceptions release assigned workers and mark failed orders appropriately.
- The monitoring loop is cancellable and runs independently of the UI. It identifies low stock and expired perishable goods without interrupting fulfilment.

## Order states

`Pending` → `Picking` → `Packed` → `Dispatched` → `Delivered`

An order may return to `Pending` if no suitable vehicle is currently available. Unexpected processing faults move it to `Failed` and return allocated staff to the workforce.
