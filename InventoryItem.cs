using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PRG_281_Project
{
        public abstract class InventoryItem
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public double Weight { get; set; }

            public int Quantity { get; set; }

            public string Zone { get; set; }

            public InventoryItem(
                int id,
                string name,
                double weight,
                int quantity,
                string zone)
            {
                Id = id;
                Name = name;
                Weight = weight;
                Quantity = quantity;
                Zone = zone;
            }

            public abstract string GetHandlingRule();
        }


        // PERISHABLE

        public class PerishableItem : InventoryItem
        {
            public DateTime ExpiryDate { get; set; }

            public PerishableItem(
                int id,
                string name,
                double weight,
                int quantity,
                string zone,
                DateTime expiryDate)
                : base(id, name, weight, quantity, zone)
            {
                ExpiryDate = expiryDate;
            }

            public override string GetHandlingRule()
            {
                return "Check expiry date.";
            }

            public bool IsExpired()
            {
                return DateTime.Now >= ExpiryDate;
            }
        }


        // FRAGILE

        public class FragileItem : InventoryItem
        {
            public FragileItem(
                int id,
                string name,
                double weight,
                int quantity,
                string zone)
                : base(id, name, weight, quantity, zone)
            {
            }

            public override string GetHandlingRule()
            {
                return "Handle carefully.";
            }
        }


        // BULK

        public class BulkItem : InventoryItem
        {
            public BulkItem(
                int id,
                string name,
                double weight,
                int quantity,
                string zone)
                : base(id, name, weight, quantity, zone)
            {
            }

            public override string GetHandlingRule()
            {
                return "Standard handling.";
            }
        }
    
}
