﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ShoeWeb.Data;
using ShoeWeb.Areas.Admin.Admin_ViewModel;
using System.Threading.Tasks;
using System.Data.Entity;
using ShoeWeb.Models;
using System.Runtime.InteropServices;
using ShoeWeb.Utility;
using ShoeWeb.Helper;

namespace ShoeWeb.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.AdminRole)]

    public class BrandController : Controller
    {
        private readonly ApplicationDbContext _db;
        public BrandController(ApplicationDbContext db)
        {
            _db = db;
        }
        public BrandController() : this(new ApplicationDbContext())
        {

        }

        public async Task<BrandVM> GetBrandVM()
        {
            return new BrandVM()
            {
                brands = await _db.brands.ToListAsync()

            };

        }
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var brands = await GetBrandVM();
            return View(brands);
        }
        [HttpPost]
        public async Task<ActionResult> DeleteBrand(int id)
        {
            try
            {

                var brand = await _db.brands.FindAsync(id);

                if (brand == null)
                {
                    return Json(new { success = false});
                }
                var isProduct = await _db.products.Where(p => p.brandId == id).FirstOrDefaultAsync();
                if (isProduct != null)
                {
                    return Json(new { success = false });
                }

                _db.brands.Remove(brand);
                await _db.SaveChangesAsync();
                var brandVM = await GetBrandVM();

                return Json(new { success = true, brands = brandVM.brands });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public async Task<ActionResult> Update(int id, string name)
        {
            try
            {
                var brand = await _db.brands.FindAsync(id);
                if (brand == null)
                {
                    return Json(new { success = false });
                }
                brand.brandName = name;
                await _db.SaveChangesAsync();

                var brandVM = await GetBrandVM();
                return Json(new { success = true, brands = brandVM.brands });
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
        }

        [HttpPost]
        public async Task<ActionResult> AddBrand(string name)
        {
            try
            {
                // Kiểm tra xem thương hiệu đã tồn tại chưa
                var existingBrand = await _db.brands.FirstOrDefaultAsync(b => b.brandName == name);
                if (existingBrand != null)
                {
                    return Json(new { success = false, message = "Thương hiệu đã tồn tại!" });
                }

                // Nếu không tồn tại, thêm mới
                Brand brand = new Brand()
                {
                    brandName = name
                };

                _db.brands.Add(brand);
                await _db.SaveChangesAsync();

                // Lấy danh sách thương hiệu mới
                var brands = await GetBrandVM();

                return Json(new { success = true, brands = brands.brands });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }




    }
}