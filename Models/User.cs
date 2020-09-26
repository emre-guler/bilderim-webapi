using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilderimApp_WebApi.Models 
{
    public class User 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public string Photo { get; set; }
        public int Money { get; set; }
        public DateTime createdAt { get; set; }
        public bool deleteState { get; set; }
        public bool userRank { get; set; }
    }
}