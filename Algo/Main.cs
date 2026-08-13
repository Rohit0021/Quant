using QuantConnect;
using QuantConnect.Algorithm;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Securities;

namespace QuantConnect
{
    public class SmaCrossDemo : QCAlgorithm
    {
        private Symbol _spy;
        private SimpleMovingAverage _fast;
        private SimpleMovingAverage _slow;

        public override void Initialize()
        {
            SetStartDate(2020, 1, 1);
            SetEndDate(2023, 1, 1);

            SetCash(100000);

            _spy = AddEquity("SPY", Resolution.Daily).Symbol;

            _fast = SMA(_spy, 20, Resolution.Daily);
            _slow = SMA(_spy, 50, Resolution.Daily);

            SetWarmUp(50, Resolution.Daily);
        }

        public override void OnData(Slice data)
        {
            if (IsWarmingUp)
                return;

            if (!_fast.IsReady || !_slow.IsReady)
                return;

            // Fast SMA crosses above slow SMA -> BUY
            if (_fast > _slow && !Portfolio[_spy].Invested)
            {
                SetHoldings(_spy, 1.0);
                Debug($"BUY {_spy} @ {Securities[_spy].Price}");
            }

            // Fast SMA crosses below slow SMA -> SELL
            else if (_fast < _slow && Portfolio[_spy].Invested)
            {
                Liquidate(_spy);
                Debug($"SELL {_spy} @ {Securities[_spy].Price}");
            }
        }
    }
}


