const args = parseArgs(process.argv);

console.log(process.argv)
console.log(args)

function parseArgs(argv) {
  const args = {};
  for (let i=2; argv[i] !== undefined; i++) {
    const val = argv[i];
    if (val.startsWith("--")) {
      const [prefix, value] = val.split("=");
      const name = prefix.split("").splice(2, prefix.length).join("");
      args[name] = value;
    }
  }
  return args;
}


