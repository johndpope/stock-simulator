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
	class BearTrendStart : Strategy
	{
		public BearTrendStart(TickerData tickerData, RunnableFactory factory) 
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
					"Trend"
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
			return "BearTrendStart";
		}

		/// <summary>
		/// Called on every new bar of data.
		/// </summary>
		/// <param name="currentBar">The current bar of the simulation</param>
		protected override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			if (currentBar < 1)
			{
				return;
			}

			Trend trend = (Trend)Dependents[0];
			if (trend.UpTrend[currentBar] == true && trend.UpTrend[currentBar - 1] == true)
			{
				WasFound[currentBar] = true;
			}
		}
	}
}
