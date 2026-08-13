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
        private Symbol _eurusd;
        private SimpleMovingAverage _fast;
        private SimpleMovingAverage _slow;

        public override void Initialize()
        {
            SetStartDate(2020, 1, 1);
            SetEndDate(2023, 1, 1);

            SetCash(100000);

            _eurusd = AddForex("EURUSD", Resolution.Hour).Symbol;
            
            _fast = SMA(_eurusd, 20, Resolution.Daily);
            _slow = SMA(_eurusd, 50, Resolution.Daily);

            SetWarmUp(50, Resolution.Daily);
        }

        public override void OnData(Slice data)
        {
            if (IsWarmingUp)
                return;

            if (!_fast.IsReady || !_slow.IsReady)
                return;

            // Fast SMA crosses above slow SMA -> BUY
            if (_fast > _slow && !Portfolio[_eurusd].Invested)
            {
                SetHoldings(_eurusd, 1.0);
                Debug($"BUY {_eurusd} @ {Securities[_eurusd].Price}");
            }

            // Fast SMA crosses below slow SMA -> SELL
            else if (_fast < _slow && Portfolio[_eurusd].Invested)
            {
                Liquidate(_eurusd);
                Debug($"SELL {_eurusd} @ {Securities[_eurusd].Price}");
            }
        }
    }
}


