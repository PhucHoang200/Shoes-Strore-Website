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

namespace ShoeWeb.Areas.Admin.Controllers
{
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
        public async Task<ActionResult> Update(int id, string name)
        {
            try
            {
                var brand = await _db.brands.FindAsync(id);
                if (brand == null) {
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
            return Json(new { success = false });
        }

    }
}