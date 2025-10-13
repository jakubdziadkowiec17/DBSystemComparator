using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class ReservationServiceCollection
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReservationId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string ServiceId { get; set; }
        [BsonElement("CreationDate")]
        public DateTime CreationDate { get; set; }
    }
}