using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilderimApp_WebApi.Models 
{
    public class Coupon 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int BetAmount { get; set; }
        public float CouponRate { get; set; }
        public DateTime CreatedAt { get; set; }
        public int ResultState { get; set; } 
        // 0 kaznamadı , 1 kazandı, 2 bekliyor
    }
}