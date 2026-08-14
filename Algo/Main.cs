using System;
using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Indicators;
using QuantConnect.Orders;

public class EmaCrossAtrStrategy : QCAlgorithm
{
    private Symbol _symbol;

    private ExponentialMovingAverage _fastEma;
    private ExponentialMovingAverage _slowEma;
    private AverageTrueRange _atr;

    // Strategy parameters
    private int _fastPeriod = 20;
    private int _slowPeriod = 50;
    private int _atrPeriod = 14;

    // Risk parameters
    private decimal _atrMultiplier = 2m;
    private decimal _rewardRisk = 3m;

    // Exit order tickets
    private OrderTicket _stopOrder;
    private OrderTicket _targetOrder;

    // ATR risk captured when the trade is entered
    private decimal _riskDistance;

    public override void Initialize()
    {
        SetStartDate(2020, 1, 1);
        SetCash(100000);

        _symbol = AddEquity("SPY", Resolution.Minute).Symbol;

        _fastEma = EMA(_symbol, _fastPeriod, Resolution.Minute);
        _slowEma = EMA(_symbol, _slowPeriod, Resolution.Minute);

        _atr = ATR(
            _symbol,
            _atrPeriod,
            MovingAverageType.Wilders,
            Resolution.Minute
        );

        SetWarmUp(_slowPeriod + _atrPeriod);
    }

    public override void OnData(Slice data)
    {
        if (IsWarmingUp)
            return;

        if (!_fastEma.IsReady ||
            !_slowEma.IsReady ||
            !_atr.IsReady)
            return;

        if (!data.Bars.ContainsKey(_symbol))
            return;

        // Don't enter another trade while already invested
        if (Portfolio[_symbol].Invested)
            return;

        // Don't enter if there are outstanding entry/exit orders
        if (_stopOrder != null || _targetOrder != null)
            return;

        // ============================
        // BULLISH EMA CROSS
        // ============================

        if (_fastEma.Current.Value > _slowEma.Current.Value &&
            _fastEma.Previous.Value <= _slowEma.Previous.Value)
        {
            EnterLong();
        }

        // ============================
        // BEARISH EMA CROSS
        // ============================

        else if (_fastEma.Current.Value < _slowEma.Current.Value &&
                 _fastEma.Previous.Value >= _slowEma.Previous.Value)
        {
            EnterShort();
        }
    }

    private void EnterLong()
    {
        var quantity = CalculateOrderQuantity(_symbol, 1.0m);

        // Capture ATR at the time of the signal
        _riskDistance = _atr.Current.Value * _atrMultiplier;

        MarketOrder(_symbol, quantity);
    }

    private void EnterShort()
    {
        var quantity = CalculateOrderQuantity(_symbol, -1.0m);

        // Capture ATR at the time of the signal
        _riskDistance = _atr.Current.Value * _atrMultiplier;

        MarketOrder(_symbol, quantity);
    }

    public override void OnOrderEvent(OrderEvent orderEvent)
    {
        if (orderEvent.Status != OrderStatus.Filled)
            return;

        // ==========================================
        // ENTRY FILLED
        // ==========================================

        if (orderEvent.OrderType == OrderType.Market &&
            Portfolio[_symbol].Invested)
        {
            var entryPrice = orderEvent.FillPrice;
            var quantity = Portfolio[_symbol].Quantity;

            if (quantity > 0)
            {
                // LONG
                var stopPrice = entryPrice - _riskDistance;
                var targetPrice =
                    entryPrice + (_riskDistance * _rewardRisk);

                _stopOrder = StopMarketOrder(
                    _symbol,
                    -quantity,
                    stopPrice
                );

                _targetOrder = LimitOrder(
                    _symbol,
                    -quantity,
                    targetPrice
                );

                Debug(
                    $"LONG ENTRY: {entryPrice:F2} | " +
                    $"SL: {stopPrice:F2} | " +
                    $"TP: {targetPrice:F2}"
                );
            }
            else if (quantity < 0)
            {
                // SHORT
                var stopPrice = entryPrice + _riskDistance;
                var targetPrice =
                    entryPrice - (_riskDistance * _rewardRisk);

                _stopOrder = StopMarketOrder(
                    _symbol,
                    -quantity,
                    stopPrice
                );

                _targetOrder = LimitOrder(
                    _symbol,
                    -quantity,
                    targetPrice
                );

                Debug(
                    $"SHORT ENTRY: {entryPrice:F2} | " +
                    $"SL: {stopPrice:F2} | " +
                    $"TP: {targetPrice:F2}"
                );
            }

            return;
        }

        // ==========================================
        // STOP LOSS FILLED
        // ==========================================

        if (_stopOrder != null &&
            orderEvent.OrderId == _stopOrder.OrderId)
        {
            Debug(
                $"STOP LOSS FILLED: {orderEvent.FillPrice:F2}"
            );

            // Cancel TP
            if (_targetOrder != null)
            {
                _targetOrder.Cancel("Stop loss triggered");
            }

            _stopOrder = null;
            _targetOrder = null;
            _riskDistance = 0;

            return;
        }

        // ==========================================
        // TAKE PROFIT FILLED
        // ==========================================

        if (_targetOrder != null &&
            orderEvent.OrderId == _targetOrder.OrderId)
        {
            Debug(
                $"TAKE PROFIT FILLED: {orderEvent.FillPrice:F2}"
            );

            // Cancel SL
            if (_stopOrder != null)
            {
                _stopOrder.Cancel("Take profit triggered");
            }

            _stopOrder = null;
            _targetOrder = null;
            _riskDistance = 0;

            return;
        }
    }
}