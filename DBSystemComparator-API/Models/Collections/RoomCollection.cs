using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class RoomCollection
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("number")]
        public int Number { get; set; }
        [BsonElement("capacity")]
        public int Capacity { get; set; }
        [BsonElement("pricePerNight")]
        public int PricePerNight { get; set; }
        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}