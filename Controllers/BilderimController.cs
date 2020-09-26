using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BilderimApp_WebApi.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BCrypt;
using Newtonsoft.Json;
using BilderimApp_WebApi.Models;
using System.Text;

namespace BilderimApp_WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BilderimController : ControllerBase
    {
        private readonly BilderimContext db;
        public BilderimController(BilderimContext context)
        {
            db = context;
        }
        [HttpPost]
        [Route("/login")]
        public string Login([FromBody] Login login)
        {
            var userControl = db.Users.Where(x => x.PhoneNumber == login.PhoneNumber).FirstOrDefault();
            if(userControl != null)
            {
                bool validPassword = BCrypt.Net.BCrypt.Verify(login.Password, userControl.Password);
                if(validPassword)
                {
                    string token = userControl.PhoneNumber + userControl.Username + userControl.createdAt;
                    token = BCrypt.Net.BCrypt.HashPassword(token);
                    var userData = db.Users.Where(x => x.ID == userControl.ID).FirstOrDefault();
                    LoginResponse rl = new LoginResponse
                    {
                        ID = userData.ID,
                        Fullname = userData.Fullname,
                        Money = userData.Money,
                        PhoneNumber = userData.PhoneNumber,
                        Photo = userData.Photo,
                        Username = userData.Username,
                        Address = userData.Address,
                        Token = token
                    };
                    string response = JsonConvert.SerializeObject(rl);
                    return response;
                }
                else 
                {
                    return "wrongEntry";
                }

            }
            else 
            {
                return "wrongEntry";
            }
        }
        [HttpPost]
        [Route("/register")]
        public string Register([FromBody] Register register)
        {
            var userControl = db.Users.Where(x => x.PhoneNumber == register.PhoneNumber || x.Username == register.UserName).FirstOrDefault();
            if(userControl == null)
            {
                register.Password = BCrypt.Net.BCrypt.HashPassword(register.Password);
                User newUser = new User 
                {
                   Username = register.UserName,
                   Fullname = register.FullName,
                   Password = register.Password,
                   PhoneNumber = register.PhoneNumber,
                   userRank = false
                };
                db.Users.Add(newUser);
                int saveControl = db.SaveChanges(); 
                if(saveControl == 1)
                {
                    return "success";
                }
                else
                {
                    return "saveControlProblem";
                }
            }
            else
            {
                return "alreadyExist";
            }
        }
        [HttpPost]
        [Route("/homepage")]
        public string HomePage()
        {
            var bulletinData = db.Bulletins.Where(x => x.State == 2 && x.ConfirmState == true ).Select(x => new {
                x.ExpaireAt,
                x.ID,
                x.Name,
                x.Tag,
                x.Photo,
                x.Explain,
                x.FirstOptionRate,
                x.SecondOptionRate
            }).OrderBy(x => x.ExpaireAt).ToList();
            List<string> responseData = new List<string>();
            foreach (var item in bulletinData)
            {
                int count = db.CouponBets.Where(x => x.BulletinID == item.ID).Count();
                BulletinItemResponse newBull = new BulletinItemResponse
                {
                    ExpaireAt = item.ExpaireAt,
                    Explain = item.Explain,
                    FirstOptionRate = item.FirstOptionRate,
                    SecondOptionRate = item.SecondOptionRate,
                    Photo = item.Photo,
                    Name = item.Name,
                    ID = item.ID,
                    UserCount = count,
                    Tag = item.Tag
                };
                string json = JsonConvert.SerializeObject(newBull);
                responseData.Add(json);
            }
            if(responseData != null)
            {
                string response = JsonConvert.SerializeObject(responseData);
                return response;
            }
            else
            {
                return "noData";
            }

        }
        [HttpPost]
        [Route("/viewBet")]
        public string ViewBet([FromBody] CouponBasketOperation viewbet)
        {
            var userTokenDB = db.Users.Where(x => x.ID == viewbet.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, viewbet.UserToken);
            if(validToken)
            {
                var basketData = db.CouponBaskets.Where(x => x.UserID == viewbet.UserID).ToList();
                List<string> bulletin = new List<string>();
                string response = "";
                foreach(var i in basketData)
                {
                    BulletinData newBull = new BulletinData();
                    if(i.BetTo)
                    {
                        var data = db.Bulletins.Where(x => x.ID == i.BulletinID).Select(x => new { ID = x.ID, Name = x.Name, Rate = x.FirstOptionRate}).FirstOrDefault();
                        newBull.ID = data.ID;
                        newBull.Name = data.Name;
                        newBull.Rate = data.Rate;
                        newBull.ResponseNote = "";
                    }
                    else
                    {
                        var data = db.Bulletins.Where(x => x.ID == i.BulletinID).Select(x => new { ID = x.ID, Name = x.Name, Rate = x.SecondOptionRate}).FirstOrDefault();
                        newBull.ID = data.ID;
                        newBull.Name = data.Name;
                        newBull.Rate = data.Rate;
                        newBull.ResponseNote = "";
                    }
                    newBull.UserMoney = userTokenDB.Money;
                    response = JsonConvert.SerializeObject(newBull);
                    bulletin.Add(response);
                }
                if(bulletin.Count > 0)
                {
                    response = JsonConvert.SerializeObject(bulletin);
                    return response;
                }
                else
                {
                    MoneyData money = new MoneyData 
                    {
                        Money = userTokenDB.Money,
                        ResponseNote = "noData"
                    };
                    response = JsonConvert.SerializeObject(money);
                    return response;
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/submitBet")]
        public string SubmitBet([FromBody] CouponBasketOperation submit)
        {
            var userTokenDB = db.Users.Where(x => x.ID == submit.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, submit.UserToken);
            if(validToken)
            {
                var basket = db.CouponBaskets.Where(x => x.UserID == submit.UserID).ToList();
                if(basket != null)
                {
                    if(userTokenDB.Money >= submit.BetAmount)
                    {
                        userTokenDB.Money = userTokenDB.Money - submit.BetAmount;
                        db.SaveChanges();
                        float couponRate = 1;
                        foreach (var item in basket)
                        {
                            var bulletinData = db.Bulletins.Where(x => x.ID == item.BulletinID).FirstOrDefault();
                            if(item.BetTo)
                            {
                                couponRate = couponRate * bulletinData.FirstOptionRate;
                            }
                            else
                            {
                                couponRate = couponRate * bulletinData.SecondOptionRate;
                            }
                        }
                        string rate = couponRate.ToString("#.##");
                        couponRate = float.Parse(rate);
                        Coupon newCoupon = new Coupon
                        {
                            BetAmount = submit.BetAmount,
                            UserID = submit.UserID,
                            CouponRate = couponRate,
                        };
                        db.Coupons.Add(newCoupon);
                        int saveControl = db.SaveChanges();
                        if(saveControl > 0)
                        {
                            foreach (var i in basket)
                            {
                                CouponBet newBet = new CouponBet
                                {
                                    BulletinID = i.BulletinID,
                                    CouponID = newCoupon.ID,
                                    BetTo = i.BetTo
                                };
                                db.CouponBets.Add(newBet);
                            }
                            saveControl = db.SaveChanges();
                            if(saveControl > 0)
                            {
                                db.CouponBaskets.RemoveRange(basket);
                                saveControl = db.SaveChanges();
                                if(saveControl > 0)
                                {
                                    return "success";
                                }
                                else
                                {
                                    return "saveControlProblem";
                                }
                            }
                            else
                            {
                                return "saveControlProblem";
                            }
                        }
                        else
                        {
                            return "saveControlProblem";
                        }
                    }
                    else
                    {
                        return "insufficientfund";
                    }
                }
                else
                {
                    return "basketNull";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/addBet")]
        public string AddBet([FromBody] CouponBasketOperation addbet)
        {
            var userTokenDB = db.Users.Where(x => x.ID == addbet.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, addbet.UserToken);
            if(validToken)
            {
                var counter = db.CouponBaskets.Where(x => x.UserID == addbet.UserID).Count();
                var sameBet = db.CouponBaskets.Where(x => x.UserID == addbet.UserID && x.BulletinID == addbet.BulletinID).FirstOrDefault();
                if(counter <= 8 && sameBet == null)
                {
                    CouponBasket newCB = new CouponBasket
                    {
                        BulletinID = addbet.BulletinID,
                        UserID = addbet.UserID,
                        BetTo = addbet.BetTo
                    };
                    db.CouponBaskets.Add(newCB);
                    int saveControl = db.SaveChanges();
                    if(saveControl == 1)
                    {
                        return "success";
                    }
                    else
                    {
                        return "saveControlProblem";
                    }
                }
                else
                {
                    return "limitSizeORsameBet";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/removeBet")]
        public string RemoveBet([FromBody] CouponBasketOperation removebet )
        {
            var userTokenDB = db.Users.Where(x => x.ID == removebet.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, removebet.UserToken);
            if(validToken)
            {
                var data = db.CouponBaskets.Where(x => x.UserID == removebet.UserID && x.BulletinID == removebet.BulletinID).FirstOrDefault();
                db.CouponBaskets.Remove(data);
                int saveControl = db.SaveChanges();
                if(saveControl == 1)
                {
                    return "success";
                }
                else
                {
                    return "saveControlProblem";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/clearBets")]
        public string ClearBets([FromBody] CouponBasketOperation removebet )
        {
            var userTokenDB = db.Users.Where(x => x.ID == removebet.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, removebet.UserToken);
            if(validToken)
            {
                var data = db.CouponBaskets.Where(x => x.UserID == removebet.UserID).ToList();
                db.CouponBaskets.RemoveRange(data);
                int saveControl = db.SaveChanges();
                if(saveControl > 0)
                {
                    return "success";
                }
                else
                {
                    return "saveControlProblem";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/viewOldCoupons")]
        public string ViewOldCoupons([FromBody] ViewOldBets bets)
        {
            var userTokenDB = db.Users.Where(x => x.ID == bets.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, bets.UserToken);
            if(validToken)
            {
                List<string> data = new List<string>();
                var Coupons = db.Coupons.Where(x => x.UserID == bets.UserID).OrderBy(x => x.CreatedAt).ToList();
                string response = "";
                foreach (var item in Coupons)
                {
                    int betsCount = db.CouponBets.Where(x => x.CouponID == item.ID).Count();
                    ViewOldBetsResponse newData = new ViewOldBetsResponse
                    {
                        ID = item.ID,
                        BetAmount = item.BetAmount,
                        BetUnit = betsCount,
                        CouponRate = item.CouponRate,
                        CouponState = item.ResultState
                    };
                    response = JsonConvert.SerializeObject(newData);
                    data.Add(response);
                }
                if(data != null)
                {
                    response = JsonConvert.SerializeObject(data);
                    return response;
                }
                else
                {
                    return "noData";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/viewOldCoupons/details")]
        public string ViewOldCouponsDetail([FromBody] ViewOldBets details)
        {
            var userTokenDB = db.Users.Where(x => x.ID == details.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, details.UserToken);
            if(validToken)
            {
                List<string> responseData = new List<string>();
                var CoupontAmount = db.Coupons.Where(x => x.ID == details.CouponID).FirstOrDefault(); 
                var CouponBetsData = db.CouponBets.Where(x => x.CouponID == details.CouponID).ToList();
                foreach (var item in CouponBetsData)
                {
                    var bulletinData = db.Bulletins.Where(x => x.ID == item.BulletinID).FirstOrDefault();
                    float betRate;
                    if(item.BetTo)
                    {
                        betRate = bulletinData.FirstOptionRate;
                    }
                    else
                    {
                        betRate = bulletinData.SecondOptionRate;
                    }
                    ViewOldBetDetailsResponse vobdr = new ViewOldBetDetailsResponse 
                    {
                        BetAmount = CoupontAmount.BetAmount,
                        BulletinRate = betRate,
                        BulletinState = bulletinData.State,
                        BulletinTitle = bulletinData.Name,
                        CouponRate = CoupontAmount.CouponRate
                    };
                    string json = JsonConvert.SerializeObject(vobdr);
                    responseData.Add(json);
                }
                if(responseData != null)
                {
                    string response = JsonConvert.SerializeObject(responseData);
                    return response;
                }
                else
                {
                    return "noData";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/mostWinner")]
        public string MostWinner()
        {
            var userData = db.Users.Where(x => x.deleteState == false).Select(x => new {x.Fullname, x.Photo, x.Money }).Take(10).ToList();
            List<string> responseData = new List<string>();
            foreach (var item in userData)
            {
                MostWinner mw = new MostWinner
                {
                    Money = item.Money,
                    Fullname = item.Fullname,
                    Photo = item.Photo
                };
                string json = JsonConvert.SerializeObject(mw);
                responseData.Add(json);
            }
            string response = JsonConvert.SerializeObject(responseData);
            return response;
        }
        [HttpPost]
        [Route("/marketlist")]
        public string MarketList()
        {
            var marketData = db.MarketLists.Where(x => x.Stock > 0 && x.deleteState == false).Select(x => new { x.ID, x.Price , x.ProductName, x.Photo, x.Explain,x.Stock}).ToList();
            List<string> responseData = new List<string>();
            foreach (var item in marketData)
            {
                MarketList ml = new MarketList
                {
                    ID = item.ID,
                    ProductName = item.ProductName,
                    Price = item.Price,
                    Photo = item.Photo,
                    Explain = item.Explain,
                    Stock = item.Stock
                };
                string json = JsonConvert.SerializeObject(ml);
                responseData.Add(json);
            }
            string response = JsonConvert.SerializeObject(responseData);
            return response;
        }
        [HttpPost]
        [Route("/buyitem")]
        public string BuyItem([FromBody] BuyItem item)
        {
            var userTokenDB = db.Users.Where(x => x.ID == item.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, item.UserToken);
            if(validToken)
            {
                var productData = db.MarketLists.Where(x => x.deleteState == false && x.Stock > 0 && x.ID == item.ProductID).FirstOrDefault();
                if(userTokenDB.Money >= productData.Price)
                {
                    userTokenDB.Money = userTokenDB.Money - productData.Price;
                    productData.Stock = productData.Stock - 1;
                    int saveControl = db.SaveChanges();
                    if(saveControl > 0)
                    {
                        BoughtItem newBought = new BoughtItem
                        {
                            ProductID = item.ProductID,
                            UserID = item.UserID
                        };
                        db.BoughtItems.Add(newBought);
                        saveControl = db.SaveChanges();
                        if(saveControl == 1)
                        {
                            return "success";
                        }
                        else
                        {
                            return "saveControlProblem";
                        }
                    }
                    else
                    {
                        return "saveControlProblem";
                    }
                }
                else
                {
                    return "insufficientfund";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/addBulletin")]
        public string AddBulletin([FromBody] NewBulletinItem item)
        {
            var userTokenDB = db.Users.Where(x => x.ID == item.CreatorUser).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, item.UserToken);
            if(validToken)
            {
                DateTime expire = DateTime.Today.AddDays(+item.ExpaireAt); 
                Bulletin newBu = new Bulletin
                {
                    CreatorUser = item.CreatorUser,
                    ExpaireAt = expire,
                    Explain = item.Explain,
                    FirstOptionRate = item.FirstOptionRate,
                    SecondOptionRate = item.SecondOptionRate,
                    Name = item.Title,
                    Photo = item.Photo,
                };
                db.Bulletins.Add(newBu);
                int saveControl = db.SaveChanges();
                if(saveControl == 1)
                {
                    return "success";
                }
                else
                {
                    return "saveControlProblem";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/profiledataupdate")]
        public string ProfileData([FromBody] Profile profile)
        {
            var userTokenDB = db.Users.Where(x => x.ID == profile.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, profile.UserToken);
            if(validToken)
            {
                var usernameControl = db.Users.Where(x => x.Username == profile.UserName || x.PhoneNumber == profile.PhoneNumber).FirstOrDefault();
                if(usernameControl == null)
                {
                    userTokenDB.Username = profile.UserName;
                    userTokenDB.Address = profile.Address;
                    userTokenDB.Fullname = profile.FullName;
                    userTokenDB.PhoneNumber = profile.PhoneNumber;
                    int saveControl = db.SaveChanges();
                    if(saveControl > 0)
                    {
                        return "success";
                    }
                    else
                    {
                        return "saveControlProblem";
                    }
                }
                else
                {
                    return "alreadyExist";
                }
            }
            else
            {
                return "tokenError";
            }
        }
        [HttpPost]
        [Route("/photoupdate")]
        public string PhotoUpdate([FromBody] UpdatePhoto uPhoto)
        {
            var userTokenDB = db.Users.Where(x => x.ID == uPhoto.UserID).FirstOrDefault();
            string token = userTokenDB.PhoneNumber + userTokenDB.Username + userTokenDB.createdAt;
            bool validToken = BCrypt.Net.BCrypt.Verify(token, uPhoto.UserToken);
            if(validToken)
            {
                userTokenDB.Photo = uPhoto.Photo;
                int saveControl = db.SaveChanges();
                if(saveControl == 1)
                {
                    return "success";
                }
                else
                {
                    return "saveControlProblem";
                }
            }
            else
            {
                return "tokenError";
            }
        }
    }
    public class Login 
    {
        public string PhoneNumber { get; set; }

        public string Password { get; set; } 
    }
    public class LoginResponse 
    {
        public int ID { get; set; }
        public string Fullname { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }
        public string Photo { get; set; }
        public string Address { get; set; }
        public int Money { get; set; }
        public string Token { get; set; }
    }
    public class Register 
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
    public class CouponBasketOperation 
    {
        public int UserID { get; set; }
        public int BulletinID { get; set; }
        public string UserToken { get; set; }
        public bool BetTo { get; set; }
        public int BetAmount { get; set; }
    }
    public class NewBulletinItem 
    {
        public int CreatorUser { get; set; }
        public string UserToken { get; set; }
        public string Title { get; set; }
        public string Explain { get; set; }
        public string Photo { get; set; }
        public float FirstOptionRate { get; set; }
        public float SecondOptionRate { get; set; }
        public int ExpaireAt { get; set; }
    }
    public class Profile 
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string UserToken { get; set; }
    }
    public class UpdatePhoto
    {
        public int UserID { get; set; }
        public string UserToken { get; set; }
        public string Photo { get; set; }
    }
    public class BuyItem 
    {
        public int UserID { get; set; }
        public int ProductID { get; set; }
        public string UserToken { get; set; }
    }
    public class BulletinData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public float Rate { get; set; }
        public int UserMoney { get; set; }
        public string ResponseNote { get; set; }
    }
    public class ViewOldBets 
    {
        public int UserID { get; set; }
        public string UserToken { get; set; }
        public int CouponID { get; set; }
    }
    public class ViewOldBetsResponse
    {
        public int ID { get; set; }
        public float CouponRate { get; set; }
        public int BetAmount { get; set; }
        public int CouponState { get; set; }
        public int BetUnit { get; set; }
    }
    public class ViewOldBetDetailsResponse 
    {
        public string BulletinTitle { get; set; }
        public float BulletinRate { get; set; }
        public int BulletinState { get; set; }
        public float CouponRate { get; set; }
        public int BetAmount { get; set; }
    }
    public class MostWinner
    {
        public string Fullname { get; set; }
        public string Photo { get; set; }
        public int Money { get; set; }
    }
    public class MarketList 
    {
        public int ID { get; set; }
        public string ProductName { get; set; }
        public int Price { get; set; }
        public string Photo { get; set; }
        public string Explain { get; set; }
        public int Stock { get; set; }
    }
    public class BulletinItem 
    {
        public int ID { get; set; }
    }
    public class BulletinItemResponse
    {
        public DateTime ExpaireAt { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Photo { get; set; }
        public string Explain { get; set; }
        public float FirstOptionRate { get; set; }
        public float SecondOptionRate { get; set; }
        public int UserCount { get; set; }
    }
    public class MoneyData
    {
        public string ResponseNote { get; set; }
        public int Money { get; set; }
    }
}