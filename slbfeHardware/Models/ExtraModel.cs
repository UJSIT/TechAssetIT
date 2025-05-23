using System.Collections.Generic;

namespace slbfeHardware.Models
{
    public class ExtraModel
    {
        //qr
        public string QrText { get; set; }
        public string QrImageBase64 { get; set; }
        public string ForegroundColor { get; set; } = "#000000";
        public string BackgroundColor { get; set; } = "#ffffff";
        public int PixelsPerModule { get; set; } = 20;
        public string ErrorCorrection { get; set; } = "Q";

        // lable
        public string ItemType { get; set; } = "";
        public int StartNumber { get; set; } = 0001;
        public int Quantity { get; set; } = 72;
        public string PaperSize { get; set; } = "A4";
        public List<List<string>> LabelGrid { get; set; } = new List<List<string>>();
        public int HorizontalSpacing { get; set; } = 10; // mm
        public int VerticalSpacing { get; set; } = 20; // mm
        public int LabelPadding { get; set; } = 5; // mm
        public int FontSize { get; set; } = 36; // pt
        public int Columns { get; set; } = 4;

    }
}
