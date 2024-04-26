using amazin.DataAccess.Repository.IRepository;
using amazin.Models;
using amazin.Models.Product;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Diagnostics;

namespace amazin.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Gender(string? gender)
        {
            Product? productGender = _unitOfWork.Product.Get(u=> u.Gender == gender);
            return View(productGender);
        }

        public IActionResult Products(int? categoryId, string? gender)
        {
            IEnumerable<Product> productList;

            if (categoryId.HasValue && !string.IsNullOrWhiteSpace(gender))
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category")
                    .Where(p => p.CategoryId == categoryId.Value && p.Gender == gender);
            }
            else if (categoryId.HasValue)
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category")
                    .Where(p => p.CategoryId == categoryId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(gender))
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category")
                    .Where(p => p.Gender == gender);
            }
            else
            {
                productList = _unitOfWork.Product.GetAll(includeProperties: "Category");
            }

            return View(productList);
        }


        public IActionResult Details(int id)
        {
            Product product = _unitOfWork.Product.Get(u=>u.Id==id,includeProperties: "Category");
            return View(product);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}