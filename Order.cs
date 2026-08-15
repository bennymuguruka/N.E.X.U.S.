using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG_281_Project
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

        public Order(
            int id,
            List<InventoryItem> items)
        {
            Id = id;
            Items = items;

            Status = OrderStatus.Pending;

            CalculateWorkers();
        }

        public double GetTotalWeight()
        {
            return Items.Sum(
                item => item.Weight * item.Quantity);
        }

        private void CalculateWorkers()
        {
            double weight = GetTotalWeight();

            // 1 picker for every 7kg

            PickersNeeded =
                (int)Math.Ceiling(weight / 7);

            DriversNeeded = 1;
        }
    }
}
