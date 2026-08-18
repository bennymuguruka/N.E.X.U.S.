using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
    public enum OrderStatus
    {
        Pending,
        Picking,
        Packed,
        Dispatched,
        Delivered,
        Failed
    }


    public class Order
    {
        public int Id { get; set; }

        public List<InventoryItem> Items { get; set; }

        public OrderStatus Status { get; set; }

        public int PickersNeeded { get; set; }

        public int DriversNeeded { get; set; }

        public string VehicleId { get; set; }

        public List<Worker> AssignedWorkers { get; set; }

        public Order(
            int id,
            List<InventoryItem> items)
        {
            Id = id;
            Items = items;
            AssignedWorkers = new List<Worker>();

            Status = OrderStatus.Pending;

            RecalculateWorkers();
        }

        public double GetTotalWeight()
        {
            return Items.Sum(
                item => item.Weight * item.Quantity);
        }

        public void RecalculateWorkers()
        {
            double weight = GetTotalWeight();

            // 1 picker for every 7kg

            PickersNeeded =
                (int)Math.Ceiling(weight / 7);

            DriversNeeded = 1;
        }
    }
}
