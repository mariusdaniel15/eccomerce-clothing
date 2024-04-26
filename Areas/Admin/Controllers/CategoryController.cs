using amazin.DataAccess.Repository.IRepository;
using amazin.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace amazin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<ProductCategory> objProductCategoryList = _unitOfWork.ProductCategory.GetAll().ToList();
            return View(objProductCategoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductCategory obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.ProductCategory.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category created successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ProductCategory? productCategoryFromDb = _unitOfWork.ProductCategory.Get(u => u.Id == id);
            if (productCategoryFromDb == null)
            {
                return NotFound();
            }
            return View(productCategoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(ProductCategory obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductCategory.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Category updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }

        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            ProductCategory? productCategoryFromDb = _unitOfWork.ProductCategory.Get(u => u.Id == id);
            if (productCategoryFromDb == null)
            {
                return NotFound();
            }
            return View(productCategoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            ProductCategory? obj = _unitOfWork.ProductCategory.Get(u => u.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            _unitOfWork.ProductCategory.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Category deleted successfully";
            return RedirectToAction("Index");
        }
    }
}
