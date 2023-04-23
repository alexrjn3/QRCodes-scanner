using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ScanApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using System.Data.SqlClient;
using System.Data;
using Microsoft.AspNetCore.Http;
using IronBarCode;
using ClosedXML.Excel;
using System.IO;

namespace ScanApp.Controllers
{
    public class HomeController : Controller
    {


        //Pentru aflare folder wwwroot
        private IHostingEnvironment Environment;

        public HomeController(IHostingEnvironment _environment)
        {
            Environment = _environment;
        }


        public string fileName;
        //Pagina Phone
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(IFormFile postedFiles)
        {
            //UPLOAD PHOTO IN WROT
            string wwwPath = this.Environment.WebRootPath;
            string path = Path.Combine(wwwPath, "Uploads");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            try
            {
                fileName = Path.GetFileName(postedFiles.FileName);
                using (FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
                {
                    postedFiles.CopyTo(stream);
                    string uploadedFile = new string(fileName);
                    ViewBag.Message = string.Format("<b>{0}</b> uploaded.<br />", fileName);
                }
            }
            catch
            {
                ViewBag.TextCatchUpload = "Upload a file!";
            }

            //READING THE BARCODE(Phone-Photo):
            var path2 = wwwPath + $"\\Uploads\\{fileName}";
            try
            {
                BarcodeResult ResultAnyFormat = BarcodeReader.QuicklyReadOneBarcode(path2);
                ViewBag.URL = $"\\Uploads\\{fileName}";
                ViewBag.Text = ResultAnyFormat.Text;
                //ViewBag.Text2 = ResultAnyFormat.BarcodeType;
            }
            catch
            {
                ViewBag.TextCatch1 = "Upload a supported barcode format, a supported file format(jpg, png, gif, tiff, svg, bmp, gif)  or make a more clear photo!";
                ViewBag.TextCatch2 = @"Supported barcode formats include: QR (+ Styled QR), Aztec, Data Matrix, MaxiCode (Read Only), USPS IM Barcode (Read Only),
                                                 Code 39, Code 128, PDF417, Rss14 (Read Only), RSS Expanded (Read Only),
                                                 UPC-A, UPC-E, EAN-8, EAN-13, Codabar, ITF, MSI, Plessey (Write Only)";
            }
            return View("Index");
        }


        //Pagina Home
        public IActionResult Home()
        {
            return View();
        }

        //Pagina Terminal
        public IActionResult Privacy()
        {
            return View();
        }


        //Partea de database(trimitere date catre db):
        Database2Controller empdb = new Database2Controller();

        [HttpGet]
        public IActionResult SendToDatabase()
        {
            return View();
        }




        [HttpPost]
        public IActionResult SendToDatabase([Bind] CodeModel codeModel)
        {
            /*
            //Delete imagini din folder wwwroot/Uploads dupa ce ne-am folosit de ele. Admin rights
            string filez = Path.Combine(Environment.WebRootPath + $"\\Uploads\\{fileName}");
            FileInfo fi = new FileInfo(filez);
            if (fi != null)
            {
                System.IO.File.Delete(filez);
                fi.Delete();
            }
            */


            try
            {
                if (ModelState.IsValid)
                {
                    string resp = empdb.AddCode(codeModel);
                    TempData["msg"] = resp;


                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = ex.Message;
            }
            return View("Home");
        }


        //LoadBarcodes() pt pagina View Barcodes(take data from db and add it to View Barcodes page)
        public static List<GetCodesModel> LoadBarcodes()
        {
            string sql = @"SELECT Material,Descriere,Cantitate FROM materialTable ORDER BY ID DESC;";
            return Database2Controller.LoadData<GetCodesModel>(sql);
        }

        //LoadBarcodesForExcel() pt pagina DownloadExcel(take data from db and add it to an Excel file)
        public static List<GetCodesModel> LoadBarcodesForExcel()
        {
            string sql = @"SELECT ID,Material,Descriere,Cantitate FROM materialTable ORDER BY ID DESC;";
            return Database2Controller.LoadData<GetCodesModel>(sql);
        }



        //Pagina View Barcodes
        public ActionResult ViewBarcodes()
        {
            ViewBag.Message = "Codes List";
            var data = LoadBarcodes();
            List<GetCodesModel> codes = new List<GetCodesModel>();
            foreach (var row in data)
            {
                codes.Add(new GetCodesModel
                {
                    Material = row.Material,
                    Descriere = row.Descriere,
                    Cantitate = row.Cantitate
                });
            }
            return View(codes);
        }

        //Pagina Download Excel
        public IActionResult DownloadExcel()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Inventory");
                worksheet.Row(1).Style.Fill.PatternType = XLFillPatternValues.Solid;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFCC00");
                worksheet.Row(2).Style.Fill.PatternType = XLFillPatternValues.Solid;
                worksheet.Row(2).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFCC00");
                worksheet.Cell(1, 1).Value = "Report";
                worksheet.Cell(1, 2).Value = "Report1";
                worksheet.Cell(2, 1).Value = "Date";
                worksheet.Cell(2, 2).Value = string.Format("{0:dd MMMM yyyy} at {0:H: mm tt}", DateTimeOffset.Now);

                worksheet.Cell(5, 1).Value = "ID";
                worksheet.Cell(5, 2).Value = "Material";
                worksheet.Cell(5, 3).Value = "Descriere";
                worksheet.Cell(5, 4).Value = "Cantitate";

                worksheet.Row(5).Style.Fill.PatternType = XLFillPatternValues.Solid;
                worksheet.Row(5).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFCC00");

                var currentRow = 6;



                var popcorn = LoadBarcodesForExcel();
                foreach (var item in popcorn)
                {
                    string material = "'" + item.Material;
                    worksheet.Row(currentRow).Style.Fill.PatternType = XLFillPatternValues.Solid;
                    worksheet.Row(currentRow).Style.Fill.BackgroundColor = XLColor.FromHtml("#FFFF99");
                    worksheet.Cell(currentRow, 1).Value = item.ID;
                    worksheet.Cell(currentRow, 2).Value = material;
                    worksheet.Cell(currentRow, 3).Value = item.Descriere;
                    worksheet.Cell(currentRow, 4).Value = item.Cantitate;

                    currentRow++;

                }
                worksheet.Columns().AdjustToContents();
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExcelReport.xlsx");
                }

            }
        }
    }
}
