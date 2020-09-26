using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BilderimApp_WebApi.Data;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BilderimApp_WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<BilderimContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddHangfire(configuration => {
                configuration.UseSqlServerStorage("Server=DESKTOP-LRLOEEE;Database=Bilderim;Trusted_Connection=True;");
            });
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            RecurringJob.AddOrUpdate(
                () =>  this.DailyWork(),
                "0 0 * * *",
                TimeZoneInfo.FindSystemTimeZoneById("Turkey Standard Time")
            );

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
        private readonly BilderimContext db;
        public Startup(BilderimContext context)
        {
            db = context;
        }
        public void DailyWork()
        {
            Console.WriteLine("It's working bro.");
            DateTime now = DateTime.Today;
            var bulletinData = db.Bulletins.Where(x => x.ConfirmState == true && x.State == 2 && x.ExpaireAt == now).ToList();
            if(bulletinData != null)
            {
                foreach (var bulletin in bulletinData)
                {
                    var bets = db.CouponBets.Where(x => x.BulletinID == bulletin.ID).ToList();
                    foreach (var bet in bets)
                    {
                        bet.State = bulletin.State;
                        db.SaveChanges();
                        var matches = db.CouponBets.Where(x => x.CouponID == bet.CouponID).ToList();
                        int win = 0;
                        int lose = 0;
                        int waitin = 0;
                        foreach (var match in matches)
                        {
                            if(match.State == 1)
                            {
                                win++;
                            }
                            else if(match.State == 0)
                            {
                                lose++;
                                break;
                            }
                            else if(match.State == 2)
                            {
                                waitin++;
                                break;
                            }
                        }
                        if(lose == 0 && waitin == 0 && win > 0)
                        {
                            var coupon = db.Coupons.Where(x => x.ID == bet.CouponID).FirstOrDefault();
                            coupon.ResultState = 1;
                            var user = db.Users.Where(x => x.ID == coupon.UserID).FirstOrDefault();
                            int calculate = Convert.ToInt32(Math.Floor(coupon.BetAmount * coupon.CouponRate));
                            user.Money = user.Money + calculate;
                        }
                        else if(lose > 0)
                        {
                            var coupon = db.Coupons.Where(x => x.ID == bet.CouponID).FirstOrDefault();
                            coupon.ResultState = 0;
                        }
                        db.SaveChanges();

                    }
                }
            }
        }
    }
}
