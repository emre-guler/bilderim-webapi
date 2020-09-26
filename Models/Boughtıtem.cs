using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilderimApp_WebApi.Models 
{
    public class BoughtItem 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public bool ShippingState { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}