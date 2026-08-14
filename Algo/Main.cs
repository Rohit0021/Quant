using System;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;

public class EmaCrossAtrStrategy : QCAlgorithm
{
    private ExponentialMovingAverage _fastEma;
    private ExponentialMovingAverage _slowEma;
    private AverageTrueRange _atr;

    private Symbol _symbol;

    // Strategy parameters
    private int _fastPeriod = 20;
    private int _slowPeriod = 50;
    private int _atrPeriod = 14;

    // Risk parameters
    private decimal _atrMultiplier = 2m;   // Stop = 2 ATR
    private decimal _rewardRisk = 3m;      // TP = 3R

    private decimal _stopPrice;
    private decimal _targetPrice;

    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        SetCash(100000);

        _symbol = AddEquity("SPY", Resolution.Minute).Symbol;

        _fastEma = EMA(_symbol, _fastPeriod, Resolution.Minute);
        _slowEma = EMA(_symbol, _slowPeriod, Resolution.Minute);
        _atr = ATR(_symbol, _atrPeriod, MovingAverageType.Wilders, Resolution.Minute);

        SetWarmUp(_slowPeriod + _atrPeriod);
    }

    public override void OnData(Slice data)
    {
        if (IsWarmingUp)
            return;

        if (!_fastEma.IsReady || !_slowEma.IsReady || !_atr.IsReady)
            return;

        if (!data.Bars.ContainsKey(_symbol))
            return;

        var price = Securities[_symbol].Price;

        // Manage existing position
        if (Portfolio[_symbol].Invested)
        {
            ManagePosition(price);
            return;
        }

        // Detect EMA crossover
        if (_fastEma.IsReady && _slowEma.IsReady)
        {
            // Bullish crossover
            if (_fastEma.Current.Value > _slowEma.Current.Value &&
                _fastEma.Previous.Value <= _slowEma.Previous.Value)
            {
                EnterLong(price);
            }

            // Bearish crossover
            else if (_fastEma.Current.Value < _slowEma.Current.Value &&
                     _fastEma.Previous.Value >= _slowEma.Previous.Value)
            {
                EnterShort(price);
            }
        }
    }

    private void EnterLong(decimal entryPrice)
    {
        var atr = _atr.Current.Value;

        // Risk = 2 ATR
        var risk = atr * _atrMultiplier;

        _stopPrice = entryPrice - risk;

        // Reward = 3 x risk
        _targetPrice = entryPrice + (risk * _rewardRisk);

        var quantity = CalculateOrderQuantity(_symbol, 1.0m);

        MarketOrder(_symbol, quantity);

        Debug($"LONG | Entry: {entryPrice:F2} | ATR: {atr:F2} | " +
              $"SL: {_stopPrice:F2} | TP: {_targetPrice:F2}");
    }

    private void EnterShort(decimal entryPrice)
    {
        var atr = _atr.Current.Value;

        // Risk = 2 ATR
        var risk = atr * _atrMultiplier;

        _stopPrice = entryPrice + risk;

        // Reward = 3 x risk
        _targetPrice = entryPrice - (risk * _rewardRisk);

        var quantity = CalculateOrderQuantity(_symbol, -1.0m);

        MarketOrder(_symbol, quantity);

        Debug($"SHORT | Entry: {entryPrice:F2} | ATR: {atr:F2} | " +
              $"SL: {_stopPrice:F2} | TP: {_targetPrice:F2}");
    }

    private void ManagePosition(decimal price)
    {
        var holdings = Portfolio[_symbol].Quantity;

        // Long position
        if (holdings > 0)
        {
            if (price <= _stopPrice)
            {
                Liquidate(_symbol, "Stop Loss");
                Debug($"LONG STOP LOSS | Price: {price:F2}");
            }
            else if (price >= _targetPrice)
            {
                Liquidate(_symbol, "Take Profit");
                Debug($"LONG TAKE PROFIT | Price: {price:F2}");
            }
        }

        // Short position
        else if (holdings < 0)
        {
            if (price >= _stopPrice)
            {
                Liquidate(_symbol, "Stop Loss");
                Debug($"SHORT STOP LOSS | Price: {price:F2}");
            }
            else if (price <= _targetPrice)
            {
                Liquidate(_symbol, "Take Profit");
                Debug($"SHORT TAKE PROFIT | Price: {price:F2}");
            }
        }
    }
}