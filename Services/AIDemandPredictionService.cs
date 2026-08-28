using Microsoft.ML;
using Microsoft.ML.Transforms.TimeSeries;
using PharmacyManagmentSystem.Data;
using PharmacyManagmentSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PharmacyManagmentSystem.Services
{
    public class AIDemandPredictionService
    {
        private readonly MLContext _mlContext;
        private readonly ApplicationDbContext _dbContext;

        public AIDemandPredictionService(ApplicationDbContext dbContext)
        {
            _mlContext = new MLContext();
            _dbContext = dbContext;
        }

        public async Task<float> PredictDemandAsync(string medicineName, int daysToPredict = 30)
        {
            // 1. Fetch historical sales data for the medicine
            var salesData = await GetHistoricalSalesDataAsync(medicineName);

            if (salesData.Count < 5)
            {
                // Not enough data for time series forecasting, return average
                return (float)salesData.Average(s => s.Quantity) * daysToPredict;
            }

            // 2. Load data into IDataView
            IDataView dataView = _mlContext.Data.LoadFromEnumerable(salesData);

            // 3. Create the forecasting pipeline (Single Spectrum Analysis)
            var forecastingPipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: "Score",
                inputColumnName: "Quantity",
                windowSize: 5,
                seriesLength: salesData.Count,
                trainSize: salesData.Count,
                horizon: 1, // predict next period (we'll group by month or week if needed, but here let's assume we predict next total)
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: "LowerBoundTimeSeries",
                confidenceUpperBoundColumn: "UpperBoundTimeSeries");

            // 4. Train the model
            var model = forecastingPipeline.Fit(dataView);

            // 5. Create a prediction engine
            var forecastingEngine = model.CreateTimeSeriesEngine<MedicineSalesData, MedicineDemandPrediction>(_mlContext);

            // 6. Predict next
            var prediction = forecastingEngine.Predict();
            
            // Score contains the forecasted values (horizon length)
            float predictedValue = prediction.ForecastedQuantity.FirstOrDefault();
            
            return Math.Max(0, predictedValue); // No negative demand
        }

        private async Task<List<MedicineSalesData>> GetHistoricalSalesDataAsync(string medicineName)
        {
            // Aggregate sales by day or week. Let's aggregate by week for better stability.
            var rawSales = await _dbContext.SaleItems
                .Include(si => si.Sale)
                .Where(si => si.MedicineName == medicineName && si.Sale != null)
                .Select(si => new { si.Sale.SaleDate, si.Quantity })
                .ToListAsync();

            // If we group by week or month:
            var groupedData = rawSales
                .GroupBy(s => new { s.SaleDate.Year, s.SaleDate.Month }) // Monthly grouping for simpler forecasting
                .Select(g => new MedicineSalesData
                {
                    Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                    Quantity = g.Sum(s => s.Quantity)
                })
                .OrderBy(s => s.Date)
                .ToList();

            return groupedData;
        }
    }
}
