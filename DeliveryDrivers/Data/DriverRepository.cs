using System.IO;
using DeliveryDrivers.Infrastructure;
using DeliveryDrivers.Models;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MotorcycleRental.RabbitService;
using Newtonsoft.Json;

namespace DeliveryDrivers.Data
{

    public interface IDriverRepository
    {
        Task SaveDriverInformations(DriverModel driver, CancellationToken cancellationToken);
        Task<bool> UpdateDriver(Guid id, DriverModel driver, CancellationToken cancellationToken);
        Task<bool> DeleteDriver(Guid id, CancellationToken cancellationToken);
        Task<DriverModelInsertDB> GetById(Guid id, CancellationToken cancellationToken);
    }

    public sealed class DriverRepository : IDriverRepository
    {
        private readonly IMongoCollection<DriverModelInsertDB> _mongodb;
        private readonly IRabbitBusService rabbitBus;
        public DriverRepository(IOptions<DeliveryDriverDatabaseSettings> configuration, IRabbitBusService rabbitBus)
        {
            var client = new MongoClient(configuration.Value.ConnectionString);
            var database = client.GetDatabase(configuration.Value.DatabaseName);
            _mongodb = database.GetCollection<DriverModelInsertDB>(configuration.Value.CollectionNameDriver);
            this.rabbitBus = rabbitBus;
        }

        public async Task<bool> DeleteDriver(Guid id, CancellationToken cancellationToken)
        {
            var existingDriver = await GetById(id, cancellationToken);

            try
            {
                var amazonS3 = new DriverimageS3();

                var uploadFile = amazonS3.DeleteFileS3(existingDriver.CnhImage);
                await uploadFile;

                await _mongodb.DeleteOneAsync(x => x.Id == id, cancellationToken);
                rabbitBus.Publish("driver-deleted");
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
        }

        public async Task<DriverModelInsertDB> GetById(Guid id, CancellationToken cancellationToken)
        {
            return await _mongodb.Find(d => d.Id == id).FirstOrDefaultAsync(cancellationToken);

        }

        public async Task SaveDriverInformations(DriverModel driver, CancellationToken cancellationToken)
        {
            var guid = Guid.NewGuid();
            driver.Id = guid;
            var key = "media/" + Guid.NewGuid();

            var existingDriver = await GetById(guid, cancellationToken);

            try
            {
                var amazonS3 = new DriverimageS3();

                var uploadFile = amazonS3.SendFileToS3("mottu-driver-image", key, driver.CnhImage);
                await uploadFile;

                if (uploadFile == null)
                {
                    throw new Exception("An error occurred while saving the image of CNH");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            var driverModelInsert = new DriverModelInsertDB
            {
                Name = driver.Name,
                Cnpj = driver.Cnpj,
                DateofBirth = driver.DateofBirth,
                Cnh = driver.Cnh,
                CnhType = driver.CnhType,
                CnhImage = key
            };


            if (existingDriver != null && driver.Id.Equals(existingDriver.Id))
            {
                throw new Exception("Driver already exists");
            }

            await _mongodb.InsertOneAsync(driverModelInsert, cancellationToken);
            rabbitBus.Publish("driver-registered");

        }

        public async Task<bool> UpdateDriver(Guid id, DriverModel driver, CancellationToken cancellationToken)
        {
            var existingDriver = await GetById(driver.Id, cancellationToken);
            try
            {
                var amazonS3 = new DriverimageS3();

                var uploadFile = amazonS3.SendFileToS3("mottu-driver-image", existingDriver.CnhImage, driver.CnhImage);
                await uploadFile;

                if (uploadFile == null)
                {
                    throw new Exception("An error occurred while saving the image of CNH");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            existingDriver.Name = driver.Name;
            existingDriver.Cnpj = driver.Cnpj;
            existingDriver.DateofBirth = driver.DateofBirth;
            existingDriver.Cnh = driver.Cnh;
            existingDriver.CnhType = driver.CnhType;
            existingDriver.CnhImage = driver.CnhImage.FileName;
            var updateResult = await _mongodb.ReplaceOneAsync(x => x.Id == id, existingDriver, cancellationToken: cancellationToken);
            rabbitBus.Publish("driver-updated");
            return true;

        }
    }
}

public class DriverModelInsertDB
{
    [BsonId]
    [BsonRepresentation(MongoDB.Bson.BsonType.String)]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Cnpj { get; set; }
    public DateTime DateofBirth { get; set; }
    public int Cnh { get; set; }
    public CnhTypeEnum CnhType { get; set; }
    public string CnhImage { get; set; }
}