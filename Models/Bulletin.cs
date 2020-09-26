using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BilderimApp_WebApi.Models 
{
    public class Bulletin 
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int CreatorUser { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Explain { get; set; }
        public string Photo { get; set; }
        public float FirstOptionRate { get; set; }
        public float SecondOptionRate { get; set; }
        public bool ConfirmState { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpaireAt { get; set; }
        public int State { get; set; } 
        // 0 hayır çıktı, 1 evet çıktı, 2 bekleniyor
    }
}