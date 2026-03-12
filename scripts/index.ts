import Chalk from "chalk";
import Figlet from "figlet";

import { runGitHook } from "@/gitHook";
import { runGraph } from "@/graph";
import { buildDoc, runDoc } from "@/docs";

// Command library
const commands: Record<string, () => void> = {
	gitHook: runGitHook,
	graph: runGraph,
	buildDoc,
	runDoc,
};

function displayHeader(message: string): void {
	console.log(
		Chalk.cyan(Figlet.textSync(message, { horizontalLayout: "full" })),
	);
}

// Main entry point
function main() {
	displayHeader("ShironT");

	const args = process.argv.slice(2);
	if (args.length === 0) {
		console.error(Chalk.red("No command provided. Available commands:"));
		console.log(Object.keys(commands).join(", "));
		process.exit(1);
	}

	const command = args[0];
	if (!commands[command]) {
		console.error(Chalk.red(`Unknown command: ${command}`));
		console.log("Available commands:", Object.keys(commands).join(", "));
		process.exit(1);
	}

	try {
		commands[command]();
	} catch (error) {
		console.error(Chalk.red(`Error running command: ${command}`));
		console.error((error as Error).message);
		process.exit(1);
	}
}

main();
