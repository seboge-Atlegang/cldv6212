// Controllers/UploadController.cs
using ABCRetailers.Models;
using ABCRetailers.Services;
using ARCRetailers;
using Microsoft.AspNetCore.Mvc;

namespace ARCRetailers.Controllers
{
    public class UploadController : Controller
    {
        private readonly IAzureStorageService _storageService;

        public UploadController(IAzureStorageService storageService)
        {
            _storageService = storageService;
        }

        public IActionResult Index()
        {
            return View(new FileUploadModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                    {
                        // Upload to Azure storage
                        var fileName = await _storageService.UpLoadFileAsync(model.ProofOfPayment, "payment-proofs");

                        // Redirect to success page
                        return RedirectToAction("Success");
                    }
                }
                catch (Exception ex)
                {
                    // Log exception and return error view
                    ModelState.AddModelError("", "File upload failed: " + ex.Message);
                    return View(model);
                }
            }

            // If ModelState is not valid, return the same view with validation errors
            return View(model);
        }

        // ✅ Add this action
        public IActionResult Success()
        {
            return View();
        }
    }
}
