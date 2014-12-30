﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSimulator.Indicators;
using StockSimulator.Strategies;
using System.Diagnostics;

namespace StockSimulator.Core
{
	/// <summary>
	/// Knows how to create other runables with and instrument and config data
	/// </summary>
	class RunnableFactory
	{
		/// <summary>
		/// Returns an new runnable object based on the name and the instrument.
		/// </summary>
		/// <param name="runnableName">Name of the runnable to create</param>
		/// <param name="instrumentData">Instrument data for this runnable to use</param>
		/// <returns>The runnable object created</returns>
		public static Runnable CreateRunnable(string runnableName, TickerData instrumentData)
		{
			Runnable createdRunnable = null;

			switch (runnableName)
			{
				// Indicators.
				case "Macd":
					createdRunnable = new Macd();
					break;

				case "Rsi":
					createdRunnable = new Rsi();
					break;

				case "Sma":
					createdRunnable = new Sma();
					break;
			
				// Strategies.
				case "BestOfSubStrategies":
					createdRunnable = new BestOfSubStrategies();
					break;

				case "MacdStrategy":
					createdRunnable = new MacdStrategy();
					break;

				case "RsiStrategy":
					createdRunnable = new RsiStrategy();
					break;

				case "SmaStrategy":
					createdRunnable = new SmaStrategy();
					break;

				default:
					Debug.WriteLine("Trying to create a runnable that doesn't exist");
					break;
			}

			return createdRunnable;
		}
	}
}