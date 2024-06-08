using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeliveryDrivers.Models
{
    public class MotorCycleModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.String)]
        public Guid Id { get; set; }
        public int Year { get; set; }
        public string Brand { get; set; }
        public string Plate { get; set; }
        public bool IsRental { get; set; }
        
    }

    public enum MotorcyclePlan
    {
        Days7 = 210,
        Days15 =  420,
        Days30 = 660,
        Days45 = 900,
        Days50 = 900
    }
}