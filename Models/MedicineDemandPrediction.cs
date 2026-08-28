using Microsoft.ML.Data;

namespace PharmacyManagmentSystem.Models
{
    public class MedicineDemandPrediction
    {
        [ColumnName("Score")]
        public float[] ForecastedQuantity { get; set; }
        
        [ColumnName("LowerBoundTimeSeries")]
        public float[] LowerBoundQuantity { get; set; }

        [ColumnName("UpperBoundTimeSeries")]
        public float[] UpperBoundQuantity { get; set; }
    }
}
