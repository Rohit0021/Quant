const { spawn } = require('child_process');
const fs = require('fs');
const os = require('os');
const path = require('path');


const argv = process.argv;
const args = parseArgs(argv);

switch (argv[2]) {
  case "backtest": {
    if (!args.algo || !args.data || !args.out || !args.name) showUseExit();
    
    dotnetBuild(args.algo);
    temp();
    break;
  }
  
  default:
    showUseExit();
}

function temp() {
  
  const dt = new Date();
  const outdir = args.out + `/${dt / 1}::${dt.toString()}`;
  console.log(outdir);
  
  const data = {
    "algorithm-type-name": "EMACross",
    "algorithm-language": "CSharp",
    "algorithm-location": `${args.algo}/bin/Debug/${args.name}.dll`,
    
    "results-destination-folder": outdir,
    
    "data-folder": args.data,
    
    "live-mode": false,
    
    "messaging-handler": "QuantConnect.Messaging.Messaging",
    "job-queue-handler": "QuantConnect.Queues.JobQueue",
    "api-handler": "QuantConnect.Api.Api",
    
    "setup-handler": "QuantConnect.Lean.Engine.Setup.ConsoleSetupHandler",
    "result-handler": "QuantConnect.Lean.Engine.Results.BacktestingResultHandler",
    "data-feed-handler": "QuantConnect.Lean.Engine.DataFeeds.FileSystemDataFeed",
    "real-time-handler": "QuantConnect.Lean.Engine.RealTime.BacktestingRealTimeHandler",
    "transaction-handler": "QuantConnect.Lean.Engine.TransactionHandlers.BacktestingTransactionHandler"
  };
  
  const tempFile = path.join(
    os.tmpdir(),
    `my-data-${Date.now()}-${process.pid}.json`
  );
  
  fs.writeFileSync(tempFile, JSON.stringify(data, null, 2));
  
  console.log(tempFile);
  
  /////////////
  
  const child = spawn('dotnet', ['/root/Quant/Lean/Launcher/bin/Debug/QuantConnect.Lean.Launcher.dll', "--config", `${tempFile}`], {
    stdio: 'inherit',
    shell: false
  });
  
  child.on('close', (code) => {
    if (code !== 0) {
      console.error(`dotnet build failed with exit code ${code}`);
      process.exitCode = code;
    }
  });
}

function dotnetBuild(projectPath) {
  const child = spawn('dotnet', ['build', projectPath], {
    stdio: 'inherit',
    shell: false
  });
  
  child.on('close', (code) => {
    if (code !== 0) {
      console.error(`dotnet build failed with exit code ${code}`);
      process.exitCode = code;
    }
  });
}

function showUseExit() {
  console.log("use: \n$ backtest --algo=/path/algo --data=/path/data --out=/path/out");
  process.exit(1);
}

function parseArgs(argv) {
  const args = {};
  for (let i = 2; argv[i] !== undefined; i++) {
    const val = argv[i];
    if (val.startsWith("--")) {
      const [prefix, value] = val.split("=");
      const name = prefix.split("").splice(2, prefix.length).join("");
      args[name] = value;
    }
  }
  return args;
}


/*******/
/*
const { spawn } = require('child_process');

const projectPath = process.argv[2];

if (!projectPath) {
  console.error('Usage: node build.js <project-path>');
  process.exit(1);
}

const child = spawn('dotnet', ['build', projectPath], {
  stdio: 'inherit',
  shell: false
});

child.on('close', (code) => {
  if (code !== 0) {
    console.error(`dotnet build failed with exit code ${code}`);
    process.exitCode = code;
  }
});
*/