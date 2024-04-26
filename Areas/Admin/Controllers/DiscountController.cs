using amazin.DataAccess.Repository.IRepository;
using amazin.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace amazin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DiscountController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public DiscountController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<ProductDiscount> objProductDiscountList = _unitOfWork.ProductDiscount.GetAll().ToList();
            return View(objProductDiscountList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductDiscount obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.ProductDiscount.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Discount created successfully";
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

            ProductDiscount? productDiscountFromDb = _unitOfWork.ProductDiscount.Get(u => u.Id == id);
            var viewModel = new ProductDiscountVM
            {
                Discount = productDiscountFromDb,
                IsActiveOptions = new List<SelectListItem>
                {
                    new SelectListItem { Text = "Active", Value = "true" },
                    new SelectListItem { Text = "Inactive", Value = "false" }
                }
            };

            if (productDiscountFromDb == null)
            {
                return NotFound();
            }
            return View(viewModel);
        }
        [HttpPost]
        public IActionResult Edit(ProductDiscountVM obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductDiscount.Update(obj.Discount);
                _unitOfWork.Save();
                TempData["success"] = "Discount updated successfully";
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
            ProductDiscount? productDiscountFromDb = _unitOfWork.ProductDiscount.Get(u => u.Id == id);
            if (productDiscountFromDb == null)
            {
                return NotFound();
            }
            return View(productDiscountFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            ProductDiscount? obj = _unitOfWork.ProductDiscount.Get(u => u.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            _unitOfWork.ProductDiscount.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Discount deleted successfully";
            return RedirectToAction("Index");
        }
    }
}

