using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilderimApp_WebApi.Models 
{
    public class CouponBasket
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int BulletinID { get; set; }
        public bool BetTo { get; set; } //  evet 1 hayır 0
    }
}