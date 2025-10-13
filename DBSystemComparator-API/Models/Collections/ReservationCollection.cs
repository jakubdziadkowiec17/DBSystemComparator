using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class ReservationCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string ClientId { get; set; }
        [BsonRepresentation(BsonType.ObjectId)]
        public string RoomId { get; set; }
        [BsonElement("CheckInDate")]
        public DateTime CheckInDate { get; set; }
        [BsonElement("CheckOutDate")]
        public DateTime CheckOutDate { get; set; }
        [BsonElement("CreationDate")]
        public DateTime CreationDate { get; set; }
    }
}