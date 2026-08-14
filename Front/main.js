// Save
// localStorage.setItem("username", "Alice");

// Read
let resfile = localStorage.getItem("--resfile");

if (!resfile) resfile = "template.json";

console.warn(resfile)

// Remove one item
// localStorage.removeItem("username");

// Clear everything
// localStorage.clear();

fetch(resfile)
  .then(r => r.json())
  .then(main);

function main(data) {
  console.log(data)
  
  document.getElementById("uid").textContent = decodeURIComponent(resfile);
  
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
  
  const stats = {
    "Accuracy": data.statistics["Win Rate"],
    "Profit Ratio": data.statistics["Profit-Loss Ratio"],
    "Frequency": data.totalPerformance.tradeStatistics.totalNumberOfTrades,
    "Expectancy": data.statistics.Expectancy,
    "Drawdown": data.statistics.Drawdown,
    "Streak": data.totalPerformance.tradeStatistics.maxConsecutiveLosingTrades,
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

fetch("/Algo/backtest/")
  .then(x => x.text())
  .then(html => {
    console.log(html)
    
    const doc = new DOMParser().parseFromString(html, "text/html");
    
    console.log(doc);
    console.log(doc.body);
    
    const directories = [...(doc.querySelector(".directories")?.querySelectorAll('a') || [])].map(x => x?.pathname);
    const files = [...(doc.querySelector(".files")?.querySelectorAll('a') || [])].map(x => x?.pathname);
    
    console.log(directories);
    console.log(files);
    
    const dirs = directories.filter(x => x.includes("Time"));
    makeSelector(dirs)
  })
  
  function makeSelector(dirs) {
    console.log(dirs)
  const select = document.createElement("select");
  
  const defaultOpt = document.createElement("option")
  defaultOpt.textContent = "Select here to load"
  select.appendChild(defaultOpt);
  
  dirs.forEach(dir => {
    const optionEl = document.createElement("option");
    
    const name = decodeURIComponent(dir).split("::")[1].split("(")[0];
    
    optionEl.value = dir;
    optionEl.textContent = name;
    
    select.appendChild(optionEl);
  });

  select.addEventListener("change", e => {
    const val = e.target.value + "EMACross.json";
    localStorage.setItem("--resfile", val);
    window.location.reload();
  })
  
  document.body.appendChild(select);
  return select;
}