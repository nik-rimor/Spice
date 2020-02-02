using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Spice.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _webHostEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db,
                                  IWebHostEnvironment webHostEnvironment)
        {
            _db = db;
            _webHostEnvironment = webHostEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }



        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync();
            return View(menuItems);
        }

        //GET - Create
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }

        //POST - Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePOST()
        {
            // get the SubCategoryId from the form request
            // since it was brought through java to the select element and not through the model to the view
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"]);

            if(!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }

            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            // Get MenuItem from db with the newly created id
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);

            // Work on the image saving section
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var uploads = "";
            var extension = "";

            if(files.Count >0 )
            {
                // files have been uploaded 
                uploads = Path.Combine(webRootPath, "images");
                extension = Path.GetExtension(files[0].FileName);

                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }                
            }
            else
            {
                // no files were uploaded, so use default image
                uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);
                extension = Path.GetExtension(uploads);
                System.IO.File.Copy(uploads, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + extension);
            }
            // Update the image property of the retrieved MenuItem from db 
            menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;

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

            var menuItem = await _db.MenuItem.Include(m=>m.Category).Include(m=>m.SubCategory).SingleOrDefaultAsync(s => s.Id == id.Value);
            if(menuItem == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = menuItem;
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == menuItem.CategoryId).ToListAsync();
            return View(MenuItemVM);
        }

        // POST - Edit
        [HttpPost,ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPOST(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }            

            if(!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }

            // Get MenuItem from database so we can compare and update values before saving again
            var menuItemFromDb = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);
            menuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            menuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            menuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            menuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            menuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;
            menuItemFromDb.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;

            // Work on the image saving section
            string webRootPath = _webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            if(files.Count > 0)
            {
                // files have been uploaded
                var uploads = Path.Combine(webRootPath, "images");
                var newFileExtension = Path.GetExtension(files[0].FileName);

                // Delete the original file, if it exists, before saving the new one

                //Get the image path of the current image from db
                var imagePath = Path.Combine(webRootPath, menuItemFromDb.Image.TrimStart('\\'));
                // Check if it exists
                if(System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                // upload the new file
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + newFileExtension), FileMode.Create))
                {
                    files[0].CopyTo(fileStream);
                }
                // Update the image property of the retrieved MenuItem from db 
                menuItemFromDb.Image = @"\images\" + MenuItemVM.MenuItem.Id + newFileExtension;
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        // GET - Details
        public async Task<IActionResult> Details(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(s => s.Id == id.Value);

            if(menuItemFromDb == null)
            {
                return NotFound();
            }

            // Get Spicy string from MenuItem.ESpicy
            MenuItemVM.MenuItem = menuItemFromDb;
            var spicyName = Enum.GetName(typeof(MenuItem.ESpicy), Int32.Parse(MenuItemVM.MenuItem.Spicyness));
            MenuItemVM.MenuItem.Spicyness = spicyName;
            return View(MenuItemVM);
        }

        // GET - Delete        
        public async Task<IActionResult> Delete(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var menuItemFromDb = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(s => s.Id == id.Value);
            if(menuItemFromDb == null)
            {
                return NotFound();
            }
            // Get Spicy string from MenuItem.ESpicy
            MenuItemVM.MenuItem = menuItemFromDb;
            var spicyName = Enum.GetName(typeof(MenuItem.ESpicy), Int32.Parse(MenuItemVM.MenuItem.Spicyness));
            MenuItemVM.MenuItem.Spicyness = spicyName;
            return View(MenuItemVM);
        }

        // POST - Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }
            var menuItemFromDb = await _db.MenuItem.FindAsync(id.Value);
            if(menuItemFromDb == null)
            {
                return NotFound();
            }

            _db.MenuItem.Remove(menuItemFromDb);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}