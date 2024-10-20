// work in progress, right now i am just working on setting up liquidity sweeps detections

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

namespace NinjaTrader.NinjaScript.Indicators
{
    public class ICTSMCstructure : Indicator
    {
        private int sweepLookback = 10;
        private double sweepThreshold = 0.1;
        private List<double> swingHighs = new List<double>();
        private List<double> swingLows = new List<double>();

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"Detects swing highs and lows using a simple pivot detection method. Stores recent swing highs and lows in lists. Checks for potential liquidity sweeps by comparing the current price action to recent swing points. Draws markers on the chart to indicate detected liquidity sweeps.";
                Name = "ICTSMCstructure";
                Calculate = Calculate.OnEachTick;
                IsOverlay = true;
                DisplayInDataBox = true;
                DrawOnPricePanel = true;
                DrawHorizontalGridLines = true;
                DrawVerticalGridLines = true;
                PaintPriceMarkers = true;
                ScaleJustification = NinjaTrader.Gui.Chart.ScaleJustification.Right;
                IsSuspendedWhileInactive = true;
            }
        }

        protected override void OnBarUpdate()
        {
            if (CurrentBar < sweepLookback) return;

            // Detect swing points
            if (IsSwingHigh())
            {
                swingHighs.Add(High[0]);
                Draw.Dot(this, "SwingHigh_" + CurrentBar, true, 0, High[0], Brushes.Red);
            }

            if (IsSwingLow())
            {
                swingLows.Add(Low[0]);
                Draw.Dot(this, "SwingLow_" + CurrentBar, true, 0, Low[0], Brushes.Green);
            }

            // Detect liquidity sweeps
            DetectLiquiditySweep();
        }

        private bool IsSwingHigh()
        {
            if (CurrentBar < 2) return false;
            return High[2] < High[1] && High[0] < High[1];
        }

        private bool IsSwingLow()
        {
            if (CurrentBar < 2) return false;
            return Low[2] > Low[1] && Low[0] > Low[1];
        }

        private void DetectLiquiditySweep()
        {
            // Look for price breaking above recent swing highs
            foreach (double swingHigh in swingHighs)
            {
                if (High[0] > swingHigh && Close[0] < swingHigh)
                {
                    Draw.Triangle(this, "HighSweep_" + CurrentBar, true, 0, High[0], Brushes.Purple);
                    Draw.Text(this, "SweepText_" + CurrentBar, true, "Liquidity Sweep", 0, High[0] + TickSize * 5, Brushes.Purple);
                }
            }

            // Look for price breaking below recent swing lows
            foreach (double swingLow in swingLows)
            {
                if (Low[0] < swingLow && Close[0] > swingLow)
                {
                    Draw.Triangle(this, "LowSweep_" + CurrentBar, true, 0, Low[0], Brushes.Purple);
                    Draw.Text(this, "SweepText_" + CurrentBar, true, "Liquidity Sweep", 0, Low[0] - TickSize * 5, Brushes.Purple);
                }
            }
        }
    }
}
