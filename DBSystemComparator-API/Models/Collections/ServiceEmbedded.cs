using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class ServiceEmbedded
    {
        [BsonElement("serviceId")]
        public ObjectId ServiceId { get; set; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("price")]
        public int Price { get; set; }
    }
}