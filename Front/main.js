fetch("template.json")
  .then(r => r.json())
  .then(main);

function main(data) {
  console.log(data);
  
  makeStats(data);
  makeCurve(data);
}

function makeStats(data) {
  const box = document.getElementById("stats-box");
  const add = (name, val) => {
    const div = document.createElement("div");
    div.classList.add(
      "flex",
      "flex-col",
      "justify-center",
      "items-center",
      "border",
      "border-1",
      "border-blue-200",
      "py-2"
    )
    
    const text = document.createElement("span");
    text.classList.add(
      "text-gray-500",
      "font-mono",
      "text-xs",
    )
    
    const value = document.createElement("span");
    div.classList.add(
      "text-black",
      "font-mono",
      "text-md",
    )
    
    text.textContent = name;
    value.textContent = val;
    div.appendChild(text);
    div.appendChild(value);
    box.appendChild(div);
  }
  
  const freq = "N.A.";
  const streak = "N.A.";
  
  const stats = {
    "Accuracy": data.statistics["Win Rate"],
    "Profit Ratio": data.statistics["Profit-Loss Ratio"],
    "Frequency": freq,
    "Expectancy": data.statistics["Expectancy"],
    "Drawdown": data.statistics["Drawdown"],
    "Streak": streak,
  };
  
  Object.keys(stats).forEach(k => {
    add(k, stats[k]);
  });
  
}

function makeCurve(data) {
  const dd =
    data.charts.Drawdown.series["Equity Drawdown"].values;
  const eq = data.charts["Strategy Equity"].series["Equity"].values;
  
  Highcharts.stockChart("chart-box", {
    rangeSelector: { enabled: false },
    navigator: { enabled: true },
    scrollbar: { enabled: true },
    xAxis: { type: 'datetime' },
    yAxis: [{
      title: { text: 'Equity' },
      height: '70%',
      opposite: false
    }, {
      title: { text: 'Drawdown' },
      top: '75%',
      height: '25%',
      offset: 0,
      opposite: false
    }],
    tooltip: {
      shared: true,
      valueDecimals: 2
    },
    series: [{
      name: 'Equity',
      type: 'line',
      data: eq,
      yAxis: 0
    }, {
      name: 'Drawdown',
      type: 'line',
      data: dd,
      yAxis: 1
    }]
  });
}

