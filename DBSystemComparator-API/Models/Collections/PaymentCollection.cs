using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class PaymentCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string ReservationId { get; set; }
        [BsonElement("Description")]
        public string Description { get; set; }
        [BsonElement("Sum")]
        public int Sum { get; set; }
        [BsonElement("CreationDate")]
        public DateTime CreationDate { get; set; }
    }
}