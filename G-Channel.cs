// For NT8
// STILL WORKING ON THIS
// BUY/SELL signals are working, logic:  channel expands while median line is crossed.
// EMA to be added to cross refrence priceaction to increase fidelity of the buy/sell signal

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
    public class GChannelTrendDetection : Indicator
    {
        private Series<double> a;
        private Series<double> b;
        private Series<double> avg;
        private Series<bool> bullish;

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Length", Description = "Length for the G-Channel", Order = 1, GroupName = "Parameters")]
        public int Length { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Crossing Signals", Description = "Show buy/sell signals", Order = 2, GroupName = "Parameters")]
        public bool ShowCross { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Fill Opacity", Description = "Opacity for channel fill (0-100)", Order = 3, GroupName = "Parameters")]
        public int FillOpacity { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Enable Sound Alerts", Description = "Play sound on signal", Order = 4, GroupName = "Parameters")]
        public bool EnableSoundAlerts { get; set; }

        [NinjaScriptProperty]
        [Display(Name = "Show Price Labels", Description = "Show price labels on signals", Order = 5, GroupName = "Parameters")]
        public bool ShowPriceLabels { get; set; }

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = "G-Channel Trend Detection Indicator";
                Name = "GChannelTrendDetection";
                Calculate = Calculate.OnBarClose;
                IsOverlay = true;
                DisplayInDataBox = true;
                PaintPriceMarkers = true;
                IsSuspendedWhileInactive = true;
                Length = 100;
                ShowCross = true;
                FillOpacity = 40;
                EnableSoundAlerts = true;
                ShowPriceLabels = true;
                IsAutoScale = true;

                AddPlot(Brushes.Blue, "Upper");
                AddPlot(Brushes.Blue, "Lower");
                AddPlot(Brushes.Lime, "Average");
            }
            else if (State == State.Configure)
            {
                a = new Series<double>(this);
                b = new Series<double>(this);
                avg = new Series<double>(this);
                bullish = new Series<bool>(this);
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < 1)
            {
                a[0] = Input[0];
                b[0] = Input[0];
                return;
            }

            // Calculate channels
            a[0] = Math.Max(Input[0], a[1]) - (a[1] - b[1]) / Length;
            b[0] = Math.Min(Input[0], b[1]) + (a[1] - b[1]) / Length;
            avg[0] = (a[0] + b[0]) / 2.0;

            // Plot values
            Values[0][0] = a[0];  // Upper
            Values[1][0] = b[0];  // Lower
            Values[2][0] = avg[0]; // Average

            // Trend detection
            bool crossUp = Close[0] > avg[0] && Close[1] <= avg[1] && (a[0] - b[0]) > (a[1] - b[1]);
            bool crossDown = Close[0] < avg[0] && Close[1] >= avg[1] && (a[0] - b[0]) > (a[1] - b[1]);
            bullish[0] = crossUp ? true : crossDown ? false : bullish[1];

            // Draw fill
            Brush fillBrush = bullish[0] ?
                new SolidColorBrush(Color.FromArgb((byte)FillOpacity, 0, 255, 0)) :
                new SolidColorBrush(Color.FromArgb((byte)FillOpacity, 255, 0, 0));

            Draw.Region(this,
                "Fill" + CurrentBar.ToString(),
                CurrentBar - 1,  // Start bar index
                CurrentBar,  // End bar index
                avg,  // Upper series
                b,  // Lower series
                null,
                fillBrush,
                FillOpacity);

            // Draw signals
            if (ShowCross && CurrentBar > 1)
            {
                if (bullish[0] && !bullish[1])
                {
                    Draw.ArrowUp(this,
                        "Buy" + CurrentBar.ToString(),
                        false,
                        0,  // Current bar
                        Low[0] - TickSize * 2,
                        Brushes.Lime);

                    if (ShowPriceLabels)
                    {
                        Draw.Text(this,
                            "BuyText" + CurrentBar.ToString(),
                            "BUY " + Close[0].ToString("N2"),
                            0,  // Current bar
                            Low[0] - TickSize * 4,
                            Brushes.Lime);
                    }

                    if (EnableSoundAlerts)
                    {
                        Alert("BuySignal", Priority.High, "Buy Signal",
                            NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav",
                            10, Brushes.Lime, Brushes.White);
                    }
                }
                else if (!bullish[0] && bullish[1])
                {
                    Draw.ArrowDown(this,
                        "Sell" + CurrentBar.ToString(),
                        false,
                        0,  // Current bar
                        High[0] + TickSize * 2,
                        Brushes.Red);

                    if (ShowPriceLabels)
                    {
                        Draw.Text(this,
                            "SellText" + CurrentBar.ToString(),
                            "SELL " + Close[0].ToString("N2"),
                            0,  // Current bar
                            High[0] + TickSize * 4,
                            Brushes.Red);
                    }

                    if (EnableSoundAlerts)
                    {
                        Alert("SellSignal", Priority.High, "Sell Signal",
                            NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert2.wav",
                            10, Brushes.Red, Brushes.White);
                    }
                }
            }
        }
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private GChannelTrendDetection[] cacheGChannelTrendDetection;
		public GChannelTrendDetection GChannelTrendDetection(int length, bool showCross, int fillOpacity, bool enableSoundAlerts, bool showPriceLabels)
		{
			return GChannelTrendDetection(Input, length, showCross, fillOpacity, enableSoundAlerts, showPriceLabels);
		}

		public GChannelTrendDetection GChannelTrendDetection(ISeries<double> input, int length, bool showCross, int fillOpacity, bool enableSoundAlerts, bool showPriceLabels)
		{
			if (cacheGChannelTrendDetection != null)
				for (int idx = 0; idx < cacheGChannelTrendDetection.Length; idx++)
					if (cacheGChannelTrendDetection[idx] != null && cacheGChannelTrendDetection[idx].Length == length && cacheGChannelTrendDetection[idx].ShowCross == showCross && cacheGChannelTrendDetection[idx].FillOpacity == fillOpacity && cacheGChannelTrendDetection[idx].EnableSoundAlerts == enableSoundAlerts && cacheGChannelTrendDetection[idx].ShowPriceLabels == showPriceLabels && cacheGChannelTrendDetection[idx].EqualsInput(input))
						return cacheGChannelTrendDetection[idx];
			return CacheIndicator<GChannelTrendDetection>(new GChannelTrendDetection(){ Length = length, ShowCross = showCross, FillOpacity = fillOpacity, EnableSoundAlerts = enableSoundAlerts, ShowPriceLabels = showPriceLabels }, input, ref cacheGChannelTrendDetection);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.GChannelTrendDetection GChannelTrendDetection(int length, bool showCross, int fillOpacity, bool enableSoundAlerts, bool showPriceLabels)
		{
			return indicator.GChannelTrendDetection(Input, length, showCross, fillOpacity, enableSoundAlerts, showPriceLabels);
		}

		public Indicators.GChannelTrendDetection GChannelTrendDetection(ISeries<double> input , int length, bool showCross, int fillOpacity, bool enableSoundAlerts, bool showPriceLabels)
		{
			return indicator.GChannelTrendDetection(input, length, showCross, fillOpacity, enableSoundAlerts, showPriceLabels);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.GChannelTrendDetection GChannelTrendDetection(int length, bool showCross, int fillOpacity, bool enableSoundAlerts, bool showPriceLabels)
		{
			return indicator.GChannelTrendDetection(Input, length, showCross, fillOpacity, enableSoundAlerts, showPriceLabels);
		}

		public Indicators.GChannelTrendDetection GChannelTrendDetection(ISeries<double> input , int length, bool showCross, int fillOpacity, bool enableSoundAlerts, bool showPriceLabels)
		{
			return indicator.GChannelTrendDetection(input, length, showCross, fillOpacity, enableSoundAlerts, showPriceLabels);
		}
	}
}

#endregion
