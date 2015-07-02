﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using StockSimulator.Core.BuySellConditions;
using StockSimulator.Indicators;

namespace StockSimulator.Strategies
{
	/// <summary>
	/// </summary>
	public class GavalasStrategy : RootSubStrategy
	{
		private class PriceZone
		{
			public double High { get; set; }
			public double Low { get; set; }
			public int NumberOfPoints { get; set; }
		}

		/// <summary>
		/// Construct the class and initialize the bar data to default values.
		/// </summary>
		/// <param name="tickerData">Ticker for the strategy</param>
		public GavalasStrategy(TickerData tickerData)
			: base(tickerData)
		{
			_dependents = new List<Runnable>()
			{
				new GavalasZones(tickerData),
				new DtOscillator(tickerData) { PeriodRsi = 8, PeriodStoch = 5, PeriodSK = 3, PeriodSD = 3 }
			};
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "GavalasStrategy";
		}

		/// <summary>
		/// </summary>
		/// <param name="currentBar">Current bar of the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 2)
			{
				return;
			}

			GavalasZones zones = (GavalasZones)_dependents[0];

			double buyDirection = zones.BuyDirection[currentBar];
			string foundStrategyName = "";

			// See if we hit one of our buy zones.
			if (buyDirection != 0.0 && zones.DidBarTouchZone(Data.Low[currentBar], Data.High[currentBar], currentBar))
			{
				// Verify with the mechanical buy signal.
				DtOscillator dtosc = (DtOscillator)_dependents[1];
				if (buyDirection > 0.0 && Data.HigherTimeframeTrend[currentBar] > 0.0)
				{
					if (dtosc.SD[currentBar] <= 25.0 && dtosc.SK[currentBar] <= 25.0)
					{
						foundStrategyName = "BullGavalasStrategy";
					}
				}
				else if (buyDirection < 0.0 && Data.HigherTimeframeTrend[currentBar] < 0.0)
				{
					if (dtosc.SD[currentBar] >= 75.0 && dtosc.SK[currentBar] >= 75.0)
					{
						foundStrategyName = "BearGavalasStrategy";
					}
				}
			}

			if (foundStrategyName.Length > 0)
			{
				List<Indicator> dependentIndicators = GetDependentIndicators();

				Order placedOrder = EnterOrder(foundStrategyName, currentBar, buyDirection, 10000,
					dependentIndicators, GetBuyConditions(), GetSellConditions());

				if (placedOrder != null)
				{
					// Get things like win/loss percent up to the point this order was started.
					StrategyStatistics orderStats = Simulator.Orders.GetStrategyStatistics(placedOrder.StrategyName,
						placedOrder.Type,
						placedOrder.Ticker.TickerAndExchange,
						currentBar,
						Simulator.Config.MaxLookBackBars);

					Bars[currentBar] = new OrderSuggestion(
						100.0,
						foundStrategyName,
						buyDirection,
						10000,
						dependentIndicators,
						new List<StrategyStatistics>() { orderStats },
						GetBuyConditions(),
						GetSellConditions());
				}
			}
		}

		/// <summary>
		/// Returns the list of buy conditions for this strategy.
		/// </summary>
		/// <returns>List of buy conditions for this strategy</returns>
		private List<BuyCondition> GetBuyConditions()
		{
			return new List<BuyCondition>()
			{
				//new OneBarTrailingHighLow(5)
				new MarketBuyCondition()
			};
		}

		/// <summary>
		/// Returns the list of sell conditions for this strategy.
		/// </summary>
		/// <returns>List of sell conditions for this strategy</returns>
		private List<SellCondition> GetSellConditions()
		{
			// Always have a max time in market and an absolute stop for sell conditions.
			return new List<SellCondition>()
			{
				new StopSellCondition(0.05),
				//new StopOneBarTrailingHighLow(),
				new MaxLengthSellCondition(5),
			};
		}

	}
}