using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace N.E.X.U.S_Warehouse_and_Logistics_Hub
{
    public abstract class Vehicle : IDispatchable, IMonitorable
    {
        public string Id { get; set; }

        public double Capacity { get; set; }

        public double CurrentLoad { get; set; }

        public bool Available { get; set; }

        public Vehicle(
            string id,
            double capacity)
        {
            Id = id;
            Capacity = capacity;
            CurrentLoad = 0;
            Available = true;
        }

        public virtual void Load(double weight)
        {
            if (CurrentLoad + weight > Capacity)
            {
                throw new CapacityExceededException(
                    $"Vehicle {Id} is overloaded.");
            }

            CurrentLoad += weight;
        }

        public abstract string GetVehicleType();

        public void Dispatch()
        {
            Available = false;

            Console.WriteLine(
                $"Vehicle {Id} dispatched.");
        }

        public void CheckStatus()
        {
            MonitorLog.Add($"{Id}: {CurrentLoad}/{Capacity}kg");
        }
    }


    // VAN

    public class Van : Vehicle
    {
        public Van(string id)
            : base(id, 1000)
        {
        }

        public override string GetVehicleType()
        {
            return "Van";
        }
    }


    // TRUCK

    public class Truck : Vehicle
    {
        public Truck(string id)
            : base(id, 5000)
        {
        }

        public override string GetVehicleType()
        {
            return "Truck";
        }
    }
}
