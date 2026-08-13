# gh repo create Quant --public --source=. --remote=origin --push

cd Back
node index.js backtest --algo=/root/Quant/Algo --name=Algo --data=/root/Quant/Static/data --out=/root/Quant/Algo/backtest

