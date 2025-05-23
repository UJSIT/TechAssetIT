using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.Mvc;
using QRCoder;
using Rotativa;
using slbfeHardware.Models;

namespace slbfeHardware.Controllers
{
    public class ExtraController : Controller
    {
        // GET: Extra/qr
        public ActionResult qr()
        {
            return View(new ExtraModel());
        }

        [HttpPost]
        public ActionResult qr(ExtraModel model)
        {
            if (string.IsNullOrEmpty(model.QrText))
            {
                TempData["Error"] = "Please enter text to generate QR code.";
                return RedirectToAction("qr");
            }

            using (var qrGenerator = new QRCodeGenerator())
            {
                QRCodeGenerator.ECCLevel eccLevel = Enum.TryParse(model.ErrorCorrection, out QRCodeGenerator.ECCLevel parsed)
                    ? parsed : QRCodeGenerator.ECCLevel.Q;

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(model.QrText, eccLevel);

                using (var qrCode = new QRCode(qrCodeData))
                {
                    Color fg = ColorTranslator.FromHtml(model.ForegroundColor);
                    Color bg = ColorTranslator.FromHtml(model.BackgroundColor);
                    using (Bitmap qrBitmap = qrCode.GetGraphic(model.PixelsPerModule, fg, bg, true))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            qrBitmap.Save(ms, ImageFormat.Png);
                            string base64Image = Convert.ToBase64String(ms.ToArray());
                            model.QrImageBase64 = "data:image/png;base64," + base64Image;
                            TempData["QrImageBytes"] = ms.ToArray();
                        }
                    }
                }
            }

            return View(model);
        }

        public ActionResult DownloadQr()
        {
            var bytes = TempData["QrImageBytes"] as byte[];
            if (bytes == null)
            {
                return RedirectToAction("qr");
            }

            return File(bytes, "image/png", "QRCode.png");
        }

        [HttpPost]
        public JsonResult GenerateQrAjax(ExtraModel model)
        {
            if (string.IsNullOrEmpty(model.QrText))
            {
                return Json(new { success = false, message = "Please enter text to generate QR code." });
            }

            using (var qrGenerator = new QRCodeGenerator())
            {
                QRCodeGenerator.ECCLevel eccLevel = Enum.TryParse(model.ErrorCorrection, out QRCodeGenerator.ECCLevel parsed)
                    ? parsed : QRCodeGenerator.ECCLevel.Q;

                QRCodeData qrCodeData = qrGenerator.CreateQrCode(model.QrText, eccLevel);

                using (var qrCode = new QRCode(qrCodeData))
                {
                    Color fg = ColorTranslator.FromHtml(model.ForegroundColor);
                    Color bg = ColorTranslator.FromHtml(model.BackgroundColor);
                    using (Bitmap qrBitmap = qrCode.GetGraphic(model.PixelsPerModule, fg, bg, true))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            qrBitmap.Save(ms, ImageFormat.Png);
                            string base64Image = Convert.ToBase64String(ms.ToArray());
                            TempData["QrImageBytes"] = ms.ToArray(); // for DownloadQr
                            return Json(new { success = true, image = "data:image/png;base64," + base64Image });
                        }
                    }
                }
            }
        }


        //End QR -----------------------------------------------------------------


        // GET: Extra/LabelGenerator
        public ActionResult LabelGenerator()
        {
            var model = new ExtraModel();
            model.LabelGrid = GenerateLabelGrid(model);
            return View(model);
        }

        [HttpPost]
        public ActionResult GenerateLabels(ExtraModel model)
        {
            model.LabelGrid = GenerateLabelGrid(model);
            return PartialView("_LabelPreview", model);
        }

        public ActionResult DownloadLabelsPdf(ExtraModel model)
        {
            model.LabelGrid = GenerateLabelGrid(model);
            return new ViewAsPdf("_LabelPrint", model)
            {
                FileName = $"Labels_{model.ItemType}_{DateTime.Now:yyyyMMddHHmmss}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageOrientation = Rotativa.Options.Orientation.Portrait,
                CustomSwitches = "--print-media-type"
            };
        }

        private List<List<string>> GenerateLabelGrid(ExtraModel model)
        {
            var grid = new List<List<string>>();
            var currentRow = new List<string>();

            for (int i = 0; i < model.Quantity; i++)
            {
                string label = $"{model.ItemType}{(model.StartNumber + i).ToString("D4")}";
                currentRow.Add(label);

                if (currentRow.Count == 5)
                {
                    grid.Add(currentRow);
                    currentRow = new List<string>();
                }
            }

            if (currentRow.Count > 0)
            {
                grid.Add(currentRow);
            }

            return grid;
        }

    }

}
