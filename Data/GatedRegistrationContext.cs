using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace HAS.Registration.Data
{
    public class GatedRegistrationContext
    {
        private readonly DbContext _db;
        private IMongoCollection<InvitedUserDAO> _users;

        public IMongoCollection<InvitedUserDAO> Users { get; }

        public GatedRegistrationContext(IConfiguration configuration)
        {
            _db = DbContext.Create(configuration["MongoDB:Identity:Database:Name"], configuration["MongoDB:Identity:ConnectionString"]);
            _users = _db.Database.GetCollection<InvitedUserDAO>("gated-registration");
            Users = _users;
        }

        [BsonIgnoreExtraElements]
        public class InvitedUserDAO
        {
            [BsonId]
            [BsonElement("_id")]
            public ObjectId Id { get; set; }
            
            [BsonElement("fname")]
            public string FirstName { get; set; }

            [BsonElement("lname")]
            public string LastName { get; set; }

            [BsonElement("email")]
            public string EmailAddress { get; set; }

            [BsonElement("i_id")]
            public string InstructorId { get; set; }

            [BsonElement("ipname")]
            public string InstructorPublicName { get; set; }

            [BsonElement("code")]
            public string EntryCode { get; set; }

            [BsonElement("isReg")]
            public bool Registered { get; set; }

            [BsonElement("isInv")]
            public bool Invited { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            [BsonElement("dregd")]
            public DateTime DateRegistered { get; set; }

            [BsonElement("logs")]
            public IEnumerable<InvitedUserLogEntryDAO> Logs { get; set; }
        }

        [BsonIgnoreExtraElements]
        public class InvitedUserLogEntryDAO
        {
            [BsonElement("code")]
            public string EntryCode { get; set; }

            [BsonElement("email")]
            public string EmailAddress { get; set; }

            [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
            [BsonElement("adate")]
            public DateTime AttemptDate { get; set; }

            [BsonElement("success")]
            public bool Success { get; set; }
            [BsonElement("rcode")]
            public int ResultCode { get; set; }
        }
    }
}
