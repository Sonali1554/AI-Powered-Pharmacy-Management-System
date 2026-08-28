using Microsoft.ML.Data;
using System;

namespace PharmacyManagmentSystem.Models
{
    public class MedicineSalesData
    {
        [LoadColumn(0)]
        public DateTime Date { get; set; }

        [LoadColumn(1)]
        public float Quantity { get; set; }
    }
}
