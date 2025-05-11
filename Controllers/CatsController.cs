using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CatManager.Data;
using CatManager.Models;
using CatManager.ViewModels;

namespace CatManager.Controllers
{
    public class CatsController : Controller
    {
        private readonly CatContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public CatsController(CatContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cats");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }

            return uniqueFileName;
        }

        private void DeleteImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                return;

            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cats", imageName);
            if (System.IO.File.Exists(imagePath))
                System.IO.File.Delete(imagePath);
        }

        public async Task<IActionResult> Index(string searchString, string searchType, bool? isAvailable, 
            string sortOrder = "name_asc")
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = sortOrder == "name_asc" ? "name_desc" : "name_asc";
            ViewData["AgeSortParm"] = sortOrder == "age_asc" ? "age_desc" : "age_asc";

            if (searchString != null)
            {
                ViewData["CurrentFilter"] = searchString;
                ViewData["SearchType"] = searchType;
                ViewData["IsAvailable"] = isAvailable;
            }

            var cats = from c in _context.Cats
                      select c;

            // Búsqueda booleana por disponibilidad
            if (isAvailable.HasValue)
            {
                cats = cats.Where(c => c.IsAvailable == isAvailable.Value);
            }

            // Búsqueda por texto según el tipo seleccionado
            if (!String.IsNullOrEmpty(searchString))
            {
                switch (searchType)
                {
                    case "name":
                        cats = cats.Where(c => c.Name.Contains(searchString));
                        break;
                    case "breed":
                        cats = cats.Where(c => c.Breed.Contains(searchString));
                        break;
                    case "color":
                        cats = cats.Where(c => c.Color.Contains(searchString));
                        break;
                    default:
                        cats = cats.Where(c =>
                            c.Name.Contains(searchString) ||
                            c.Breed.Contains(searchString) ||
                            c.Color.Contains(searchString) ||
                            c.Description.Contains(searchString));
                        break;
                }
            }

            // Ordenamiento
            switch (sortOrder)
            {
                case "name_desc":
                    cats = cats.OrderByDescending(c => c.Name);
                    break;
                case "age_asc":
                    cats = cats.OrderBy(c => c.Age);
                    break;
                case "age_desc":
                    cats = cats.OrderByDescending(c => c.Age);
                    break;
                default: // name_asc
                    cats = cats.OrderBy(c => c.Name);
                    break;
            }

            return View(await cats.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CatViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var cat = new Cat
                {
                    Name = viewModel.Name,
                    Breed = viewModel.Breed,
                    Age = viewModel.Age,
                    Description = viewModel.Description,
                    Color = viewModel.Color,
                    IsAvailable = viewModel.IsAvailable,
                    IsKidFriendly = viewModel.IsKidFriendly
                };

                if (viewModel.ImageFile != null)
                {
                    cat.ImageUrl = await SaveImage(viewModel.ImageFile);
                }

                _context.Add(cat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cat = await _context.Cats.FindAsync(id);
            if (cat == null)
            {
                return NotFound();
            }

            var viewModel = new CatViewModel
            {
                Id = cat.Id,
                Name = cat.Name,
                Breed = cat.Breed,
                Age = cat.Age,
                Description = cat.Description,
                Color = cat.Color,
                IsAvailable = cat.IsAvailable,
                IsKidFriendly = cat.IsKidFriendly
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CatViewModel viewModel)
        {
            if (id != viewModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var cat = await _context.Cats.FindAsync(id);
                    if (cat == null)
                    {
                        return NotFound();
                    }

                    cat.Name = viewModel.Name;
                    cat.Breed = viewModel.Breed;
                    cat.Age = viewModel.Age;
                    cat.Description = viewModel.Description;
                    cat.Color = viewModel.Color;
                    cat.IsAvailable = viewModel.IsAvailable;
                    cat.IsKidFriendly = viewModel.IsKidFriendly;

                    if (viewModel.ImageFile != null)
                    {
                        DeleteImage(cat.ImageUrl);
                        cat.ImageUrl = await SaveImage(viewModel.ImageFile);
                    }

                    _context.Update(cat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CatExists(viewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        private bool CatExists(int id)
        {
            return _context.Cats.Any(e => e.Id == id);
        }
    }
}
