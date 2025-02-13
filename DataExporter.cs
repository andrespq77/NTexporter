using System;
using System.IO;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.Strategies;
using NinjaTrader.Data;

namespace NinjaTrader.Custom.Strategies
{
    public class ExportToCSV : Strategy
    {
        private StreamWriter writer;
        private string filePath = Path.Combine(NinjaTrader.Core.Globals.UserDataDir, "dataexporter/NinjaTraderData.csv");

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Calculate = Calculate.OnBarClose;
                Name = "ExportToCSV";
            }
            else if (State == State.Configure)
            {
                AddDataSeries(Data.BarsPeriodType.Minute, 60); // A�adir serie de datos de 60 minutos

                if (!File.Exists(filePath))
                {
                    using (writer = new StreamWriter(filePath, true))
                    {
                        writer.WriteLine("time,open,high,low,close,VWMA,RSI,RSI-based MA,MOM,CHOP,%K,%D,ATR,Volume");
                    }
                }
            }
        }

        protected override void OnBarUpdate()
        {
            if (BarsInProgress != 1) return; // Asegurarse de que estamos en la serie de 60 minutos
            if (CurrentBar < 14) return;

            double vwma = VWMA(14)[0];
            double rsi = RSI(14, 3)[0];
            double rsiMA = SMA(RSI(14, 3), 3)[0];
            double momentum = Momentum(14)[0];
            double atr = ATR(14)[0];
            double stochK = Stochastics(14, 3, 3).K[0];
            double stochD = Stochastics(14, 3, 3).D[0];
            double chop = ChoppinessIndex(14)[0]; // Usar el indicador nativo de NinjaTrader

            // Convertir el tiempo a datetime64[ns] sin la zona horaria UTC
            string time = Time[0].ToString("yyyy-MM-dd HH:mm:ss.ffffff");

            using (writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{time},{Open[0]},{High[0]},{Low[0]},{Close[0]},{vwma},{rsi},{rsiMA},{momentum},{chop},{stochK},{stochD},{atr},{Volume[0]}");
            }
        }
    }
}
