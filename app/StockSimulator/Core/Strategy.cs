﻿using StockSimulator.Core.BuySellConditions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockSimulator.Core
{
	public class Strategy : Runnable
	{
		/// <summary>
		/// Holds whether the strategy had a buy signal for each bar.
		/// </summary>
		public List<bool> WasFound { get; set; }

		/// <summary>
		/// Expose the order type of this strategy.
		/// </summary>
		public double OrderType { get { return _orderType; } }

		/// <summary>
		/// The type of orders this strategy places.
		/// </summary>
		protected double _orderType;

		/// <summary>
		/// List of all the orders that are not closed.
		/// </summary>
		private List<Order> _activeOrders { get; set; }

		/// <summary>
		/// Constructor to initialize the strategy.
		/// </summary>
		/// <param name="tickerData">Data for the ticker involved</param>
		public Strategy(TickerData tickerData) 
			: base(tickerData)
		{
			WasFound = Enumerable.Repeat(false, tickerData.NumBars).ToList();
			_activeOrders = new List<Order>();
			_orderType = Order.OrderType.Long; // Default to long orders.
		}

		/// <summary>
		/// Calls update for any dependent strategies and then runs the dependent indicators
		/// for the require bars so they are up to date for this bar. Indicators need to be
		/// run everytime because they can change with new bar data. For example, the ZigZag
		/// indicator finds high and low swings and draws a line between them. On one bar, a
		/// high might seem like the highest, but then a few bars later it's not and the line
		/// needs to be updated. The strategy that depends on that data needs to see the updated view.
		/// Oh and also updates the orders that are from this strategy.
		/// </summary>
		/// <param name="currentBar">The current bar in the simulation</param>
		public override void OnBarUpdate(int currentBar)
		{
			base.OnBarUpdate(currentBar);

			// Run any dependent strategies.
			for (int i = 0; i < _dependents.Count; i++)
			{
				if (_dependents[i] is Strategy)
				{
					_dependents[i].OnBarUpdate(currentBar);
				}
			}

			OnBeforeIndicatorRun(currentBar);

			// Now run any dependent indicators that this strategy needs.
			for (int i = 0; i < _dependents.Count; i++)
			{
				if (_dependents[i] is Indicator)
				{
					Indicator ind = (Indicator)_dependents[i];
					ind.Initialize();
					ind.RunToBar(currentBar);
					ind.Shutdown();
				}
			}

			// Update all the open orders.
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				Order order = _activeOrders[i];
				order.Update(currentBar);
			}

			// Remove the orders that are finished. This will just remove them from
			// this array but they order will still be saved in the order history.
			_activeOrders.RemoveAll(order => order.IsFinished() || order.Status == Order.OrderStatus.Cancelled);
		}

		/// <summary>
		/// Called before the indicators are run so the strategy can update
		/// any values in the dependent indicators before they start. Things like
		/// configurable periods and such and such.
		/// </summary>
		/// <param name="currentBar">The current bar in the simulation</param>
		protected virtual void OnBeforeIndicatorRun(int currentBar)
		{
		}

		/// <summary>
		/// Creates and enters an order depending on the type of orders this strategy places (Long or Short)
		/// </summary>
		/// <param name="strategyName">Name of the strategy that placed the order</param>
		/// <param name="currentBar">Bar the order was placed on</param>
		/// <param name="orderType">Type of order to place (long or short)</param>
		/// <param name="sizeOfOrder">Amount of money to place in this order</param>
		/// <param name="dependentIndicators">List of all the dependent indicators</param>
		/// <param name="buyConditions">All the buy conditions that must be met to fill the order</param>
		/// <param name="sellConditions">Any of the sell conditions trigger a sell</param>
		/// <returns>The order that was placed or null if none was placed</returns>
		protected Order EnterOrder(
			string strategyName,
			int currentBar,
			double orderType,
			double sizeOfOrder,
			List<Indicator> dependentIndicators,
			List<BuyCondition> buyConditions,
			List<SellCondition> sellConditions)
		{
			Order order = null;

			// Find the number of open orders this strategy current has.
			int openOrders = 0;
			for (int i = 0; i < _activeOrders.Count; i++)
			{
				if (_activeOrders[i].StrategyName == strategyName)
				{
					++openOrders;
				}
			}

			// Only place the order if it's less than the allowed amount of concurrent orders allowed.
			if (openOrders < Simulator.Config.MaxConcurrentOrders)
			{
				// Save only the names of the indicators for the order to track. The order manager
				// will use the actual indicators to save what they look like when this order
				// was placed.
				List<string> indicatorNames = new List<string>();
				for (int i = 0; i < dependentIndicators.Count; i++)
				{
					indicatorNames.Add(dependentIndicators[i].ToString());
				}

				order = new Order(
					orderType,
					Data,
					strategyName,
					currentBar,
					sizeOfOrder,
					indicatorNames,
					buyConditions,
					sellConditions);

				Simulator.Orders.AddOrder(order, dependentIndicators, currentBar);
				_activeOrders.Add(order);
			}

			return order;
		}

	}
}
