﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using StockSimulator.Core;
using StockSimulator.Indicators;

namespace StockSimulator.Strategies
{
	class BullStochasticsFastCrossover : Strategy
	{
		public BullStochasticsFastCrossover(TickerData tickerData, RunnableFactory factory)
			: base(tickerData, factory)
		{

		}

		/// <summary>
		/// Returns an array of dependent names.
		/// </summary>
		public override string[] DependentNames
		{
			get
			{
				string[] deps = {
					"StochasticsFast"
				};

				return deps;
			}
		}

		/// <summary>
		/// Returns the name of this strategy.
		/// </summary>
		/// <returns>The name of this strategy</returns>
		public override string ToString()
		{
			return "BullStochasticsFastCrossover";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			StochasticsFast ind = (StochasticsFast)Dependents[0];
			if (DataSeries.IsBelow(ind.D, 20, currentBar, 0) != -1)
			{
				if (DataSeries.CrossAbove(ind.K, ind.D, currentBar, 0) != -1)
				{
					WasFound[currentBar] = true;
				}
			}
		}
	}
}
