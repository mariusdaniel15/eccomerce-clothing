using amazin.DataAccess.Data;
using amazin.DataAccess.Repository.IRepository;
using amazin.Models.Admin;
using amazin.Models.User;
using Microsoft.AspNetCore.Mvc;

namespace amazin.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public UserController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            List<User> objUserList = _unitOfWork.User.GetAll().ToList();
            return View(objUserList);
        }

        public IActionResult Upsert(int? id)
        {
            if (id == null || id == 0)
            {
                var newUserViewModel = new UserInfoViewModel
                {
                    User = new User(),
                    Address = new UserAddress(),
                    Payment = new UserPayment()
                };

                return View(newUserViewModel);
            }
            else
            {
                // Edit
                var userToBeEdited = _unitOfWork.User.Get(u => u.Id == id);

                if (userToBeEdited == null)
                {
                    return NotFound();
                }

                var userAddress = _unitOfWork.UserAddress.Get(c => c.UserId == id);

                var userPayment = _unitOfWork.UserPayment.Get(d => d.UserId == id);

                var userViewModel = new UserInfoViewModel
                {
                    User = userToBeEdited,
                    Address = userAddress,
                    Payment = userPayment
                };

                return View(userViewModel);
            }
        }
        [HttpPost]
        public IActionResult Upsert(UserInfoViewModel viewModel)
        {
            // Check if it's a new user or an existing user
            bool isNewUser = viewModel.User.Id == 0;

            if (isNewUser)
            {
                // For new users, enforce password validation
                if (string.IsNullOrWhiteSpace(viewModel.User.Password))
                {
                    ModelState.AddModelError("User.Password", "The Password field is required.");
                }
                else if (viewModel.User.Password.Length < 8)
                {
                    ModelState.AddModelError("User.Password", "Password must be at least 8 characters.");
                }

                if (viewModel.User.Username != null && viewModel.User.Username.Equals(viewModel.User.Password, StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("User.Password", "Password can't exactly match the Username.");
                }
            }
            else
            {
                ModelState.Remove("User.Password");
                if (ModelState.IsValid)
                {
                    if (isNewUser)
                    {
                        // Create a new user
                        var newUser = viewModel.User; // User entity from view model

                        // Add the new user to the database
                        _unitOfWork.User.Add(newUser);
                        _unitOfWork.Save();

                        // Assign the user ID to the address and payment entities
                        var newAddress = viewModel.Address; // Address entity from view model
                        newAddress.UserId = newUser.Id;
                        var newPayment = viewModel.Payment; // Payment entity from view model
                        newPayment.UserId = newUser.Id;

                        // Add address and payment entities to the database
                        _unitOfWork.UserAddress.Add(newAddress);
                        _unitOfWork.UserPayment.Add(newPayment);
                        _unitOfWork.Save();
                    }
                    else
                    {
                        // Update an existing user
                        // Retrieve the existing user entity from the database and update its properties
                        var existingUser = _unitOfWork.User.Get(u => u.Id == viewModel.User.Id);
                        existingUser.Username = viewModel.User.Username;
                        existingUser.FirstName = viewModel.User.FirstName;
                        existingUser.LastName = viewModel.User.LastName;
                        existingUser.Telephone = viewModel.User.Telephone;
                        if (string.IsNullOrWhiteSpace(viewModel.User.Password))
                        {
                            viewModel.User.Password = existingUser.Password;
                        }

                        // Update the user entity in the database
                        _unitOfWork.User.Update(existingUser);
                        _unitOfWork.Save();

                        // Retrieve the existing address and payment entities from the database
                        var existingAddress = _unitOfWork.UserAddress.Get(c => c.UserId == viewModel.User.Id);
                        var existingPayment = _unitOfWork.UserPayment.Get(d => d.UserId == viewModel.User.Id);

                        // Update the properties of the existing address and payment entities
                        existingAddress.AddressLine1 = viewModel.Address.AddressLine1;
                        existingAddress.AddressLine2 = viewModel.Address.AddressLine2;
                        existingAddress.City = viewModel.Address.City;
                        existingAddress.Country = viewModel.Address.Country;
                        existingAddress.PostalCode = viewModel.Address.PostalCode;

                        existingPayment.PaymentType = viewModel.Payment.PaymentType;
                        existingPayment.Provider = viewModel.Payment.Provider;
                        existingPayment.AccountNo = viewModel.Payment.AccountNo;
                        existingPayment.Expiry = viewModel.Payment.Expiry;

                        // Update the address and payment entities in the database
                        _unitOfWork.UserAddress.Update(existingAddress);
                        _unitOfWork.UserPayment.Update(existingPayment);
                        _unitOfWork.Save();
                    }

                    TempData["success"] = isNewUser ? "User created successfully" : "User updated successfully";
                    return RedirectToAction("Index");
                }
            }
            return View(viewModel);
        }


        public IActionResult Info(int id)
        {
            var user = _unitOfWork.User.Get(u => u.Id == id);

            var userAddress = _unitOfWork.UserAddress.Get(c => c.UserId == id);

            var userPayment = _unitOfWork.UserPayment.Get(d => d.UserId == id);

            var viewModel = new UserInfoViewModel
            {
                User = user,
                Address = userAddress,
                Payment = userPayment
            };

            return View(viewModel);
        }

        #region APICALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<User> objUserList = _unitOfWork.User.GetAll().ToList();
            return Json(new { data = objUserList });
        }

        [HttpDelete]
        public IActionResult Delete(int? id)
        {
            var userToBeDeleted = _unitOfWork.User.Get(u => u.Id == id);
            if (userToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }

            _unitOfWork.User.Remove(userToBeDeleted);
            _unitOfWork.Save();


            return Json(new { success = true, message = "Delete Successful" });
        }
        #endregion

    }
}
