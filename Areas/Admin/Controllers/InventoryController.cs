using amazin.DataAccess.Repository.IRepository;
using amazin.Models.Product;
using Microsoft.AspNetCore.Mvc;

namespace amazin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class InventoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public InventoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<ProductInventory> objProductInventoryList = _unitOfWork.ProductInventory.GetAll().ToList();
            return View(objProductInventoryList);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(ProductInventory obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.ProductInventory.Add(obj);
                _unitOfWork.Save();
                TempData["success"] = "Inventory created successfully";
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
            ProductInventory? productInventoryFromDb = _unitOfWork.ProductInventory.Get(u => u.Id == id);
            if (productInventoryFromDb == null)
            {
                return NotFound();
            }
            return View(productInventoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(ProductInventory obj)
        {
            if (ModelState.IsValid)
            {
                _unitOfWork.ProductInventory.Update(obj);
                _unitOfWork.Save();
                TempData["success"] = "Inventory updated successfully";
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
            ProductInventory? productInventoryFromDb = _unitOfWork.ProductInventory.Get(u => u.Id == id);
            if (productInventoryFromDb == null)
            {
                return NotFound();
            }
            return View(productInventoryFromDb);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {
            ProductInventory? obj = _unitOfWork.ProductInventory.Get(u => u.Id == id);
            if (id == null || id == 0)
            {
                return NotFound();
            }
            _unitOfWork.ProductInventory.Remove(obj);
            _unitOfWork.Save();
            TempData["success"] = "Inventory deleted successfully";
            return RedirectToAction("Index");
        }
    }
}