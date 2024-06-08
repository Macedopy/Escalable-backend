using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DeliveryDrivers.Models
{
    public class DriverModel
    
    {
        [BsonId]
        [BsonRepresentation(MongoDB.Bson.BsonType.String)]
        public Guid Id {get; set; }
        public string Name {get; set; }
        public string Cnpj {get; set; }
        public DateTime DateofBirth {get; set; }
        public int Cnh {get; set; }
        public CnhTypeEnum CnhType {get; set; }
        public IFormFile CnhImage {get; set; }
        

    }

    public enum CnhTypeEnum
    {
        A,
        B,
        AB
    }
}