using amazin.DataAccess.Repository.IRepository;
using amazin.Models.Admin;
using amazin.Models.Product;
using amazin.Models.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace amazin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<AdminUser> objAdminList = _unitOfWork.AdminUser.GetAll(includeProperties:"Type").ToList();
            return View(objAdminList);
        }

        public IActionResult Upsert(int? id)
        {
            AdminDataEditViewModel viewModel = new()
            {
                AdminTypeList = _unitOfWork.AdminType
                .GetAll().Select(u => new SelectListItem
                {
                    Text = u.TypeAdmin,
                    Value = u.Id.ToString()
                }),
                AdminUser = new AdminUser(),
            };

            if (id == null || id == 0)
            {
                //create
                return View(viewModel);
            }
            else
            {
                //update
                viewModel.AdminUser = _unitOfWork.AdminUser.Get(u => u.Id == id);
                return View(viewModel);
            }
        }

        [HttpPost]
        public IActionResult Upsert(AdminDataEditViewModel obj)
        {
            // Check if it's a new admin or an existing admin
            bool isNewAdmin = obj.AdminUser.Id == 0;

            if (isNewAdmin)
            {
                // For new admins, enforce password validation
                if (string.IsNullOrWhiteSpace(obj.AdminUser.Password))
                {
                    ModelState.AddModelError("AdminUser.Password", "The Password field is required.");
                }
                else if (obj.AdminUser.Password.Length < 8)
                {
                    ModelState.AddModelError("AdminUser.Password", "Password must be at least 8 characters.");
                }

                if (obj.AdminUser.Username != null && obj.AdminUser.Username.Equals(obj.AdminUser.Password, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("AdminUser.Password", "Password can't exactly match the Username.");
                }
            }
            else
            {
                ModelState.Remove("AdminUser.Password"); // Remove the password field from validation
            }

            if (ModelState.IsValid)
            {
                if (isNewAdmin)
                {
                    // Create a new admin
                    var newAdmin = obj.AdminUser; // AdminUser entity from view model

                    // Add the new admin to the database
                    _unitOfWork.AdminUser.Add(newAdmin);
                    _unitOfWork.Save();
                }
                else
                {
                    // Update an existing admin
                    // Retrieve the existing admin entity from the database and update its properties
                    var existingAdmin = _unitOfWork.AdminUser.Get(u => u.Id == obj.AdminUser.Id);
                    existingAdmin.Username = obj.AdminUser.Username;
                    existingAdmin.FirstName = obj.AdminUser.FirstName;
                    existingAdmin.LastName = obj.AdminUser.LastName;
                    existingAdmin.TypeId = obj.AdminUser.TypeId;

                    // If the password is empty, keep the existing password; otherwise, update it
                    if (!string.IsNullOrWhiteSpace(obj.AdminUser.Password))
                    {
                        existingAdmin.Password = obj.AdminUser.Password;
                    }

                    // Update the admin entity in the database
                    _unitOfWork.AdminUser.Update(existingAdmin);
                    _unitOfWork.Save();
                }

                TempData["success"] = isNewAdmin ? "Admin created successfully" : "Admin updated successfully";
                return RedirectToAction("Index");
            }
            else
            {
                obj.AdminTypeList = _unitOfWork.AdminType
                    .GetAll().Select(u => new SelectListItem
                    {
                        Text = u.TypeAdmin,
                        Value = u.Id.ToString()
                    });
            }
            return View(obj);
        }


        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<AdminUser> objAdminUserList = _unitOfWork.AdminUser.GetAll(includeProperties:"Type").ToList();
            return Json(new { data = objAdminUserList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var adminToBeDeleted = _unitOfWork.AdminUser.Get(u => u.Id == id);
            if (adminToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.AdminUser.Remove(adminToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion
    }
}
