using System.Data;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ShortUrl.Common.Utility.ToolsHelpers;

namespace ShortUrl.Common.Utility.ToolsHelpers
{
    public class ExcelTools
    {
        private readonly ExcelHorizontalAlignment _defaultAlignment;

        public ExcelTools(bool isRightToLeft = true)
        {
            // تنظیم License - برای EPPlus.Core لازم است
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            _defaultAlignment = isRightToLeft ?
                ExcelHorizontalAlignment.Right :
                ExcelHorizontalAlignment.Left;
        }

        /// <summary>
        /// ایجاد فایل اکسل جدید از DataTable
        /// </summary>
        public bool CreateExcelFromDataTable(DataTable dataTable,
                                           string filePath,
                                           string sheetName = "Sheet1",
                                           List<string> rightAlignColumns = null,
                                           bool applyHeaderStyle = true)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    // بارگذاری داده از DataTable
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                    // تنظیم استایل‌ها
                    ApplyWorksheetStyles(worksheet, dataTable, rightAlignColumns, applyHeaderStyle);

                    // ذخیره فایل
                    package.SaveAs(new FileInfo(filePath));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در ایجاد فایل اکسل: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// ایجاد فایل اکسل از لیست آبجکت‌ها
        /// </summary>
        public bool CreateExcelFromList<T>(List<T> data,
                                         string filePath,
                                         string sheetName = "Sheet1",
                                         List<string> rightAlignColumns = null,
                                         bool applyHeaderStyle = true)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    // بارگذاری داده از لیست
                    worksheet.Cells["A1"].LoadFromCollection(data, true);

                    // تنظیم استایل‌ها
                    ApplyWorksheetStyles(worksheet, data, rightAlignColumns, applyHeaderStyle);

                    // ذخیره فایل
                    package.SaveAs(new FileInfo(filePath));
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در ایجاد فایل اکسل: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// خواندن فایل اکسل و تبدیل به DataTable
        /// </summary>
        public DataTable ReadExcelToDataTable(string filePath,
                                            string sheetName = null,
                                            bool hasHeader = true)
        {
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet;

                    if (string.IsNullOrEmpty(sheetName))
                        worksheet = package.Workbook.Worksheets[0];
                    else
                        worksheet = package.Workbook.Worksheets[sheetName];

                    if (worksheet == null)
                        throw new Exception("Worksheet مورد نظر یافت نشد");

                    return WorksheetToDataTable(worksheet, hasHeader);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در خواندن فایل اکسل: {ex.Message}");
                return new DataTable();
            }
        }

        /// <summary>
        /// اضافه کردن شیت جدید به فایل اکسل موجود
        /// </summary>
        public bool AddSheetToExcel(DataTable dataTable,
                                  string filePath,
                                  string sheetName,
                                  List<string> rightAlignColumns = null,
                                  bool applyHeaderStyle = true)
        {
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    // بررسی وجود شیت با همین نام
                    if (package.Workbook.Worksheets.Any(ws => ws.Name == sheetName))
                    {
                        package.Workbook.Worksheets.Delete(sheetName);
                    }

                    var worksheet = package.Workbook.Worksheets.Add(sheetName);

                    // بارگذاری داده
                    worksheet.Cells["A1"].LoadFromDataTable(dataTable, true);

                    // تنظیم استایل‌ها
                    ApplyWorksheetStyles(worksheet, dataTable, rightAlignColumns, applyHeaderStyle);

                    // ذخیره فایل
                    package.Save();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در اضافه کردن شیت: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// اعمال استایل‌های راست‌چین و سایر استایل‌ها
        /// </summary>
        private void ApplyWorksheetStyles(ExcelWorksheet worksheet,
                                        DataTable dataTable,
                                        List<string> rightAlignColumns,
                                        bool applyHeaderStyle)
        {
            // تنظیم جهت راست‌به‌چپ
            worksheet.View.RightToLeft = true;

            if (applyHeaderStyle)
            {
                // استایل هدر
                var headerRange = worksheet.Cells[1, 1, 1, dataTable.Columns.Count];
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // تعیین ستون‌های راست‌چین
            if (rightAlignColumns == null)
            {
                // به صورت پیش‌فرض تمام ستون‌ها راست‌چین می‌شوند
                var dataRange = worksheet.Cells[2, 1, dataTable.Rows.Count + 1, dataTable.Columns.Count];
                dataRange.Style.HorizontalAlignment = _defaultAlignment;
            }
            else
            {
                // راست‌چین کردن ستون‌های مشخص شده
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    var columnName = dataTable.Columns[col].ColumnName;
                    var alignment = rightAlignColumns.Contains(columnName) ?
                        ExcelHorizontalAlignment.Right :
                        ExcelHorizontalAlignment.Left;

                    var columnRange = worksheet.Cells[2, col + 1, dataTable.Rows.Count + 1, col + 1];
                    columnRange.Style.HorizontalAlignment = alignment;
                }
            }

            // تنظیم عرض خودکار ستون‌ها
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// اعمال استایل‌ها برای لیست آبجکت‌ها
        /// </summary>
        private void ApplyWorksheetStyles<T>(ExcelWorksheet worksheet,
                                           List<T> data,
                                           List<string> rightAlignColumns,
                                           bool applyHeaderStyle)
        {
            // تنظیم جهت راست‌به‌چپ
            worksheet.View.RightToLeft = true;

            if (applyHeaderStyle && data.Count > 0)
            {
                var properties = typeof(T).GetProperties();
                var headerRange = worksheet.Cells[1, 1, 1, properties.Length];

                headerRange.Style.Font.Bold = true;
                headerRange.Style.Font.Color.SetColor(Color.White);
                headerRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerRange.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
                headerRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // اعمال تراز برای داده‌ها
            if (data.Count > 0)
            {
                var properties = typeof(T).GetProperties();

                for (int col = 0; col < properties.Length; col++)
                {
                    var propertyName = properties[col].Name;
                    var alignment = (rightAlignColumns == null || rightAlignColumns.Contains(propertyName)) ?
                        ExcelHorizontalAlignment.Right :
                        ExcelHorizontalAlignment.Left;

                    var dataRange = worksheet.Cells[2, col + 1, data.Count + 1, col + 1];
                    dataRange.Style.HorizontalAlignment = alignment;
                }
            }

            // تنظیم عرض خودکار ستون‌ها
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }

        /// <summary>
        /// تبدیل Worksheet به DataTable
        /// </summary>
        private DataTable WorksheetToDataTable(ExcelWorksheet worksheet, bool hasHeader)
        {
            var dataTable = new DataTable();
            var startRow = hasHeader ? 2 : 1;

            // اضافه کردن ستون‌ها
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                var columnName = hasHeader ?
                    worksheet.Cells[1, col].Value?.ToString() ?? $"Column{col}" :
                    $"Column{col}";

                dataTable.Columns.Add(columnName);
            }

            // اضافه کردن سطرهای داده
            for (int row = startRow; row <= worksheet.Dimension.End.Row; row++)
            {
                var dataRow = dataTable.NewRow();
                var hasData = false;

                for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
                {
                    var cellValue = worksheet.Cells[row, col].Value;
                    dataRow[col - 1] = cellValue?.ToString() ?? string.Empty;

                    if (cellValue != null)
                        hasData = true;
                }

                if (hasData)
                    dataTable.Rows.Add(dataRow);
            }

            return dataTable;
        }

        /// <summary>
        /// تنظیم عرض ستون‌ها
        /// </summary>
        public bool SetColumnWidths(string filePath,
                                  Dictionary<string, double> columnWidths,
                                  string sheetName = null)
        {
            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    ExcelWorksheet worksheet;

                    if (string.IsNullOrEmpty(sheetName))
                        worksheet = package.Workbook.Worksheets[0];
                    else
                        worksheet = package.Workbook.Worksheets[sheetName];

                    if (worksheet == null)
                        return false;

                    foreach (var kvp in columnWidths)
                    {
                        var columnIndex = GetColumnIndexByName(worksheet, kvp.Key);
                        if (columnIndex != -1)
                        {
                            worksheet.Column(columnIndex).Width = kvp.Value;
                        }
                    }

                    package.Save();
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"خطا در تنظیم عرض ستون‌ها: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// پیدا کردن index ستون بر اساس نام
        /// </summary>
        private int GetColumnIndexByName(ExcelWorksheet worksheet, string columnName)
        {
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                if (worksheet.Cells[1, col].Value?.ToString() == columnName)
                    return col;
            }
            return -1;
        }
    }
}


//مثال ۱: ایجاد اکسل از DataTable

//csharp
//// ایجاد DataTable نمونه
//var dataTable = new DataTable("Products");
//dataTable.Columns.Add("نام محصول", typeof(string));
//dataTable.Columns.Add("قیمت", typeof(decimal));
//dataTable.Columns.Add("تعداد", typeof(int));

//// اضافه کردن داده
//dataTable.Rows.Add("لپ‌تاپ", 15000000, 10);
//dataTable.Rows.Add("ماوس", 500000, 25);

//// ایجاد اکسل
//var excelTools = new ExcelTools();
//var result = excelTools.CreateExcelFromDataTable(
//    dataTable,
//    "products.xlsx",
//    "محصولات",
//    new List<string> { "قیمت", "تعداد" } // ستون‌های راست‌چین
//);
//مثال ۲: ایجاد اکسل از لیست آبجکت‌ها

//csharp
//public class Product
//{
//    public string نام { get; set; }
//    public decimal قیمت { get; set; }
//    public int تعداد { get; set; }
//}

//var products = new List<Product>
//{
//    new Product { نام = "کیبورد", قیمت = 800000, تعداد = 15 },
//    new Product { نام = "مانیتور", قیمت = 5000000, تعداد = 8 }
//};

//var excelTools = new ExcelTools();
//var result = excelTools.CreateExcelFromList(
//    products,
//    "products.xlsx",
//    "محصولات",
//    new List<string> { "قیمت", "تعداد" }
//);
//مثال ۳: خواندن اکسل

//csharp
//var excelTools = new ExcelTools();
//var dataTable = excelTools.ReadExcelToDataTable("products.xlsx", "محصولات");

//foreach (DataRow row in dataTable.Rows)
//{
//    Console.WriteLine($"نام: {row["نام"]}, قیمت: {row["قیمت"]}");
//}
