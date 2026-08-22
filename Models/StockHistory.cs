namespace PharmacyManagmentSystem.Models
{
    public class StockHistory
    {
        public int StockHistoryID { get; set; }

        public string BatchNumber { get; set; } = string.Empty;

        public int MedicineID { get; set; }

        public int QuantityChange { get; set; }

        public string Action { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;
    }
}