using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class ClientCollection
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        [BsonElement("FirstName")]
        public string Name { get; set; }
        [BsonElement("SecondName")]
        public string SecondName { get; set; }
        [BsonElement("LastName")]
        public string LastName { get; set; }
        [BsonElement("Email")]
        public string Email { get; set; }
        [BsonElement("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        [BsonElement("Address")]
        public string Address { get; set; }
        [BsonElement("PhoneNumber")]
        public string PhoneNumber { get; set; }
        [BsonElement("IsActive")]
        public bool IsActive { get; set; }
    }
}