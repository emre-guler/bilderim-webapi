using BilderimApp_WebApi.Models;
using Microsoft.EntityFrameworkCore;
namespace BilderimApp_WebApi.Data
{
    public class BilderimContext : DbContext 
    {
        public BilderimContext (DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Bulletin>().Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");
            builder.Entity<Bulletin>().Property(x => x.ConfirmState).HasDefaultValue(false);
            builder.Entity<Bulletin>().Property(x => x.State).HasDefaultValue(2);

            builder.Entity<User>().Property(x => x.createdAt).HasDefaultValueSql("getDate()");
            builder.Entity<User>().Property(x => x.deleteState).HasDefaultValue(false);
            builder.Entity<User>().Property(x => x.Money).HasDefaultValue(100);

            builder.Entity<Coupon>().Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");
            builder.Entity<Coupon>().Property(x => x.ResultState).HasDefaultValue(2);

            builder.Entity<CouponBet>().Property(x => x.State).HasDefaultValue(2);


            builder.Entity<MarketList>().Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");
            builder.Entity<MarketList>().Property(x => x.deleteState).HasDefaultValue(0);

            builder.Entity<BoughtItem>().Property(x => x.ShippingState).HasDefaultValue(false);
            builder.Entity<BoughtItem>().Property(x => x.CreatedAt).HasDefaultValueSql("getDate()");
        }
        public DbSet<CouponBet> CouponBets { get; set; }
        public DbSet<Bulletin> Bulletins { get; set; }
        public DbSet<CouponBasket> CouponBaskets { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<MarketList> MarketLists { get; set; }
        public DbSet<BoughtItem> BoughtItems { get; set; }
    }
}