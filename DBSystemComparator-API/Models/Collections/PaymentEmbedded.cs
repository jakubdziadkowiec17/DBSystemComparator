using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class PaymentEmbedded
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("description")]
        public string Description { get; set; }
        [BsonElement("sum")]
        public int Sum { get; set; }
        [BsonElement("creationDate")]
        public DateTime CreationDate { get; set; }
    }
}