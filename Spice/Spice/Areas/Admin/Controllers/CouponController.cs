using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _db.Coupon.ToListAsync();
            return View(coupons);
        }

        // GET - Create
        public IActionResult Create()
        {
            return View();
        }

        // POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon coupon)
        {
            if(!ModelState.IsValid)
            {
                return View(coupon);
            }

            var files = HttpContext.Request.Form.Files;
            if(files.Count >0)
            {
                byte[] p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                coupon.Picture = p1;
            }
            _db.Coupon.Add(coupon);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if(id == null)
            { 
                return NotFound();
            }

            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id.Value);
            if(couponFromDb == null)
            {
                return NotFound();
            }

            return View(couponFromDb);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon viewCoupon)
        {
            if(!ModelState.IsValid)
            {
                return View(viewCoupon);
            }

            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == viewCoupon.Id);
            if(couponFromDb == null)
            {
                return NotFound();
            }

            // update onject from db
            couponFromDb.Name = viewCoupon.Name;
            couponFromDb.Discount = viewCoupon.Discount;
            couponFromDb.CouponType = viewCoupon.CouponType;
            couponFromDb.MinimumAmount = viewCoupon.MinimumAmount;
            couponFromDb.IsActive = viewCoupon.IsActive;

            // check for file picture upload to update current
            var files = HttpContext.Request.Form.Files;
            if(files.Count > 0)
            {
                byte[] p1 = null;
                using (var fs1 = files[0].OpenReadStream())
                {
                    using (var ms1 = new MemoryStream())
                    {
                        fs1.CopyTo(ms1);
                        p1 = ms1.ToArray();
                    }
                }
                couponFromDb.Picture = p1;
            }
            _db.Coupon.Update(couponFromDb);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // Get - Details
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id.Value);

            if(couponFromDb == null)
            {
                return NotFound();
            }
            var couponType = Enum.GetName(typeof(Coupon.EcouponType), Int32.Parse(couponFromDb.CouponType));
            couponFromDb.CouponType = couponType;
            return View(couponFromDb);
        }

        //Get - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id.Value);
            if(couponFromDb == null)
            {
                return NotFound();
            }
            var couponType = Enum.GetName(typeof(Coupon.EcouponType), Int32.Parse(couponFromDb.CouponType));
            couponFromDb.CouponType = couponType;
            return View(couponFromDb);
        }

        //POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id.Value);
            if (couponFromDb == null)
            {
                return NotFound();
            }
            _db.Coupon.Remove(couponFromDb);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}