using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilderimApp_WebApi.Models 
{
    public class CouponBet 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int CouponID { get; set; }
        public int BulletinID { get; set; }
        public bool BetTo { get; set; } //  hayır 0, evet 1 
        public int State { get; set; } 
        // kazanamadı 0, 1 kazandı , 2 bekleniyo
    }
}