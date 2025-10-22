using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DBSystemComparator_API.Models.Collections
{
    public class ClientCollection
    {
        [BsonId]
        public ObjectId Id { get; set; }
        [BsonElement("firstName")]
        public string FirstName { get; set; }
        [BsonElement("secondName")]
        public string SecondName { get; set; }
        [BsonElement("lastName")]
        public string LastName { get; set; }
        [BsonElement("email")]
        public string Email { get; set; }
        [BsonElement("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
        [BsonElement("phoneNumber")]
        public string PhoneNumber { get; set; }
        [BsonElement("isActive")]
        public bool IsActive { get; set; }
    }
}