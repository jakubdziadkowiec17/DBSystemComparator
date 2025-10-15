using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class RoomCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("Number")]
        public int Number { get; set; }
        [BsonElement("Capacity")]
        public int Capacity { get; set; }
        [BsonElement("PricePerNight")]
        public int PricePerNight { get; set; }
        [BsonElement("IsActive")]
        public bool IsActive { get; set; }
    }
}