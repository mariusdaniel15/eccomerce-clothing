using amazin.DataAccess.Repository.IRepository;
using amazin.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace amazin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return View(objProductList);
        }

        public IActionResult Upsert(int? id)
        {

            ProductDataEditViewModel viewModel = new()
            {
                CategoriesList = _unitOfWork.ProductCategory
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                InventoryList = _unitOfWork.ProductInventory
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Quantity.ToString(),
                    Value = u.Id.ToString()
                }),
                DiscountList = _unitOfWork.ProductDiscount
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                }),
                Product = new Product(),
            };
            if(id==null || id == 0)
            {
                //create
                return View(viewModel);
            }
            else
            {
                //update
                viewModel.Product = _unitOfWork.Product.Get(u => u.Id == id);
                return View(viewModel);
            }
        }

        [HttpPost]
        public IActionResult Upsert(ProductDataEditViewModel obj, IFormFile? file)
        {
            if (obj.Product.Name == obj.Product.SKU)
            {
                ModelState.AddModelError("name", "The name can't exactly match the SKU.");
            }

            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if (file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");

                    if (!string.IsNullOrEmpty(obj.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, obj.Product.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using(var fileStream = new FileStream(Path.Combine(productPath, fileName),FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }

                    obj.Product.ImageUrl = @"\images\product\" + fileName;
                }
                

                if(obj.Product.Id == 0)
                {
                    _unitOfWork.Product.Add(obj.Product);
                }
                else
                {
                    _unitOfWork.Product.Update(obj.Product);

                }
                _unitOfWork.Save();
                TempData["success"] = "Product created successfully";
                return RedirectToAction("Index");
            } 
            else
            {
                obj.CategoriesList = _unitOfWork.ProductCategory
                 .GetAll().Select(u => new SelectListItem
                 {
                     Text = u.Name,
                     Value = u.Id.ToString()
                 });
                obj.InventoryList = _unitOfWork.ProductInventory
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Quantity.ToString(),
                    Value = u.Id.ToString()
                });
                obj.DiscountList = _unitOfWork.ProductDiscount
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.Name,
                    Value = u.Id.ToString()
                });
            }

            return View(obj);
        }

        public IActionResult Info(int id)
        {
            var product = _unitOfWork.Product.Get(u => u.Id == id);

            var categoryId = product.CategoryId;
            var category = _unitOfWork.ProductCategory.Get(c => c.Id == categoryId);

            var discountId = product.DiscountId;
            var discount = _unitOfWork.ProductDiscount.Get(d => d.Id == discountId);

            var inventoryId = product.InventoryId;
            var inventory = _unitOfWork.ProductInventory.Get(e => e.Id == inventoryId);

            var viewModel = new ProductInfoViewModel
            {
                Product = product,
                Category = category,
                Discount = discount,
                Inventory = inventory
            };

            return View(viewModel);
        }

        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.Product.GetAll(includeProperties:"Category").ToList();
            return Json(new {data = objProductList});
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var productToBeDeleted = _unitOfWork.Product.Get(u => u.Id == id);
            if(productToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            var oldImagePath = 
                Path.Combine(_webHostEnvironment.WebRootPath,
                productToBeDeleted.ImageUrl.TrimStart('\\'));

            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }

            _unitOfWork.Product.Remove(productToBeDeleted);
            _unitOfWork.Save();

            
            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
