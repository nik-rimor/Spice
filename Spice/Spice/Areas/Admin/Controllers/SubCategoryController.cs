using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        [TempData]
        public string StatusMessage { get; set; }


        private readonly ApplicationDbContext _db;

        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET - Index
        public async Task<IActionResult> Index()
        {
            var subCategories = await _db.SubCategory.Include(s => s.Category).ToListAsync();
            return View(subCategories);
        }

        //GET - Create
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel createViewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await _db.SubCategory.
                                        OrderBy(p => p.Name).
                                        Select(p => p.Name).
                                        Distinct().ToListAsync()
            };

            return View(createViewModel);
        }

        // POST - Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel createViewModel)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category)
                    .Where(s => s.Name == createViewModel.SubCategory.Name && s.Category.Id == createViewModel.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    // Error
                    StatusMessage = "Error: Subcategory exists under "
                        + doesSubCategoryExists.First().Category.Name
                        + " category. Please use another name.";
                }
                else
                {
                    _db.SubCategory.Add(createViewModel.SubCategory);
                    await _db.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel newViewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = createViewModel.SubCategory,
                SubCategoryList = await _db.SubCategory.
                                        OrderBy(p => p.Name).
                                        Select(p => p.Name).
                                        Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(newViewModel);
        }


        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();

            subCategories = await (from subCategory in _db.SubCategory
                                   where subCategory.CategoryId == id
                                   select subCategory).ToListAsync();

            return Json(new SelectList(subCategories, "Id", "Name"));
        }

        // GET - Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            SubCategoryAndCategoryViewModel createViewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = subCategory,
                SubCategoryList = await _db.SubCategory.
                                        OrderBy(p => p.Name).
                                        Select(p => p.Name).
                                        Distinct().ToListAsync()
            };

            return View(createViewModel);
        }

        // POST - Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubCategoryAndCategoryViewModel editViewModel)
        {
            if (ModelState.IsValid)
            {
                var doesSubCategoryExists = _db.SubCategory.Include(s => s.Category)
                    .Where(s => s.Name == editViewModel.SubCategory.Name && s.Category.Id == editViewModel.SubCategory.CategoryId);
                if (doesSubCategoryExists.Count() > 0)
                {
                    // Error
                    StatusMessage = "Error: Subcategory exists under "
                        + doesSubCategoryExists.First().Category.Name
                        + " category. Please use another name.";
                }
                else
                {
                    var subCatFromDb = await _db.SubCategory.SingleOrDefaultAsync(s => s.Id == editViewModel.SubCategory.Id);
                    subCatFromDb.Name = editViewModel.SubCategory.Name;

                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel newViewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _db.Category.ToListAsync(),
                SubCategory = editViewModel.SubCategory,
                SubCategoryList = await _db.SubCategory.
                                        OrderBy(p => p.Name).
                                        Select(p => p.Name).
                                        Distinct().ToListAsync(),
                StatusMessage = StatusMessage
            };

            return View(newViewModel);
        }


        // GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id.Value);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        // GET - Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id.Value);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        // POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subCategory = await _db.SubCategory.FindAsync(id.Value);
            if (subCategory == null)
            {
                return NotFound();
            }

            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


    }
}
            
        
   