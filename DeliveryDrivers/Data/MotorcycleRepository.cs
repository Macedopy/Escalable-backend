using DeliveryDrivers.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MotorcycleRental.RabbitService;

namespace MotorcycleRental.Data
{
    public interface IMotorcycleService
    {
        Task RegisterMotorcycle(MotorCycleModel motorcycleModel, CancellationToken cancellationToken);
        Task<bool> DeleteMotorcycle(string plate, CancellationToken cancellationToken);
        Task<bool> UpdateMotorcycle(MotorCycleModel motorcycleModel, CancellationToken cancellationToken);
        Task<List<MotorCycleModel>> GetMotorcycle(MotorCycleModel motorCycleModel, CancellationToken cancellationToken);
        Task<MotorCycleModel> GetMotorcyclePlate(string plate, CancellationToken cancellationToken);
        Task<List<MotorCycleModel>> GetMotorcycleAvailable(MotorCycleModel motorcycleModel, CancellationToken cancellationToken);
        Task<bool> SetRentalAvailable(string plate, CancellationToken cancellationToken);
        Task<bool> SetRentalNotAvailable(string plate, CancellationToken cancellationToken);
    }

    public sealed class MotorcycleRepository : IMotorcycleService
    {
        private readonly IMongoCollection<MotorCycleModel> _mongodb;
        private readonly IRabbitBusService rabbitBus;

        public MotorcycleRepository(IOptions<DeliveryDriverDatabaseSettings> configuration, IRabbitBusService rabbitBus)
        {
            var client = new MongoClient(configuration.Value.ConnectionString);
            var database = client.GetDatabase(configuration.Value.DatabaseName);
            _mongodb = database.GetCollection<MotorCycleModel>("Motorcycle");
            this.rabbitBus = rabbitBus;
        }

        public async Task<bool> DeleteMotorcycle(string plate,CancellationToken cancellationToken)
        {
            var existingDriver = await GetMotorcyclePlate(plate.ToUpper(), cancellationToken);

            if (existingDriver == null)
            {
                throw new Exception("Motorcycle with this Plate not exists");
            }

            var updateResult = await _mongodb.DeleteOneAsync(m => m.Id == existingDriver.Id, cancellationToken);
            rabbitBus.Publish("motorcycle-deleted");

            return true;
        }

        public async Task<List<MotorCycleModel>> GetMotorcycle(MotorCycleModel motorCycleModel, CancellationToken cancellationToken)
        {
            var result = await _mongodb.Find(Builders<MotorCycleModel>.Filter.Empty).Sort(Builders<MotorCycleModel>.Sort.Ascending(m => m.Plate)).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<List<MotorCycleModel>> GetMotorcycleAvailable(MotorCycleModel motorcycleModel, CancellationToken cancellationToken)
        {
            var result = await _mongodb.Find(Builders<MotorCycleModel>.Filter.Empty).Sort(Builders<MotorCycleModel>.Sort.Descending(m => m.IsRental)).ToListAsync(cancellationToken);

            return result;
        }

        public async Task<MotorCycleModel> GetMotorcyclePlate(string plate, CancellationToken cancellationToken)
        {
            return await _mongodb.Find(m => m.Plate == plate.ToUpper()).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task RegisterMotorcycle(MotorCycleModel motorcycleModel, CancellationToken cancellationToken)
        {
            var guid = Guid.NewGuid();
            motorcycleModel.Id = guid;

            var existingDriver = await GetMotorcyclePlate(motorcycleModel.Plate, cancellationToken);
            motorcycleModel.Plate = motorcycleModel.Plate.ToUpper();
            if (existingDriver != null && !motorcycleModel.Plate.Equals(existingDriver.Plate))
            {
                throw new Exception("Motorcycle with this Plate already exists");
            }
            await _mongodb.InsertOneAsync(motorcycleModel, cancellationToken);
            rabbitBus.Publish("motorcycle-registered");
        }

        public async Task<bool> SetRentalAvailable(string plate, CancellationToken cancellationToken)
        {
            var existingDriver = await GetMotorcyclePlate(plate.ToUpper(), cancellationToken);

            if (existingDriver == null)
            {
                throw new Exception("Motorcycle with this Plate not exists");
            }

            var updateResult = await _mongodb.UpdateOneAsync(m => m.Plate == plate, Builders<MotorCycleModel>.Update.Set(m => m.IsRental, true), cancellationToken: cancellationToken);
            rabbitBus.Publish("motorcycle-setavailable");

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> SetRentalNotAvailable(string plate, CancellationToken cancellationToken)
        {
            var existingDriver = await GetMotorcyclePlate(plate.ToUpper(), cancellationToken);

            if (existingDriver == null)
            {
                throw new Exception("Motorcycle with this Plate not exists");
            }

            var updateResult = await _mongodb.UpdateOneAsync(m => m.Plate == plate, Builders<MotorCycleModel>.Update.Set(m => m.IsRental, false), cancellationToken: cancellationToken);
            rabbitBus.Publish("motorcycle-setnotavailable");

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }

        public async Task<bool> UpdateMotorcycle(MotorCycleModel motorcycleModel, CancellationToken cancellationToken)
        {
            var existingDriver = await GetMotorcyclePlate(motorcycleModel.Plate.ToUpper(), cancellationToken);

            if (existingDriver == null)
            {
                throw new Exception("Motorcycle with this Plate not exists");
            }

            var updateResult = await _mongodb.ReplaceOneAsync(m => m.Id == motorcycleModel.Id, motorcycleModel, cancellationToken: cancellationToken);
            rabbitBus.Publish("motorcycle-updated");

            return updateResult.IsAcknowledged && updateResult.ModifiedCount > 0;
        }
    }
}