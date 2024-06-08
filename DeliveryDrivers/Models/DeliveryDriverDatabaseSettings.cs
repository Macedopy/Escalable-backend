using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DeliveryDrivers.Models
{
    public class DeliveryDriverDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
        public string CollectionNameDriver { get; set; } = null!;
        public string CollectionNameMotorcyle { get; set; } = null!;

    }
}