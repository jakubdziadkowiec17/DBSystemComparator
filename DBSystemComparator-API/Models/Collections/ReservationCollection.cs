using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class ReservationCollection
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("client")]
        public ClientCollection Client { get; set; }
        [BsonElement("room")]
        public RoomCollection Room { get; set; }
        [BsonElement("checkInDate")]
        public DateTime CheckInDate { get; set; }
        [BsonElement("checkOutDate")]
        public DateTime? CheckOutDate { get; set; }
        [BsonElement("creationDate")]
        public DateTime CreationDate { get; set; }
        [BsonElement("services")]
        public List<ServiceCollection> Services { get; set; } = new();
        [BsonElement("payments")]
        public List<PaymentEmbedded> Payments { get; set; } = new();
    }
}