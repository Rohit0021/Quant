const { spawn } = require('child_process');

const args = parseArgs(process.argv);

console.log(process.argv[2])
console.log(args)
console.log(args.algo)
console.log(Boolean(args.algo))
console.log(!args.algo)

switch (args) {
  case "backtest": {
    console.log('case "backtest"')
    if (!args.algo /*|| !args.data || !args.out*/ ) {
      console.log('if (!args.algo)')
      showUseExit();
    }
    console.log('case "backtest" after if')
    dotnetBuild(args.algo);
    
    break;
  }
  
  default: {
    showUseExit();
  }
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