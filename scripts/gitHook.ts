import Chalk from "chalk";
import Figlet from "figlet";

import { existsSync } from "node:fs";
import path from "node:path"

import { runCommand, scanForSlnFile } from "@/utils";
import { buildDoc } from "@/docs";

// --- Configuration ---
const repoRoot = path.join(process.cwd(), "..");
const solutionFile = scanForSlnFile(repoRoot) ?? "";

// --- Types ---
enum LogLevel {
	INFO = "INFO",
	ERROR = "ERROR",
}

// --- Utility Functions ---
function logMessage(level: LogLevel, message: string): void {
	const prefix = `[ShironT]`;
	const formattedMessage = `${prefix} ${message}`;

	if (level === LogLevel.INFO) {
		console.log(Chalk.green(formattedMessage));
	} else if (level === LogLevel.ERROR) {
		console.error(Chalk.red(formattedMessage));
	}
}

export async function runGitHook() {
	try {
		// 1. Verify solution file exists
		if (!existsSync(solutionFile)) {
			throw new Error(`Solution file not found at: ${solutionFile}`);
		}

		logMessage(LogLevel.INFO, "Fixing whitespace issues...");
		const whitespaceFix = runCommand("dotnet format whitespace");
		if (!whitespaceFix.success) {
			throw new Error("Whitespace fix failed.");
		}

		// 2. Format Check
		logMessage(LogLevel.INFO, "Checking code format...");
		const formatCheck = runCommand(
			"dotnet format --verify-no-changes --verbosity minimal",
		);
		if (!formatCheck.success) {
			throw new Error("Format check failed. Run 'dotnet format' to fix.");
		}

		// 3. Build the entire solution once
		logMessage(LogLevel.INFO, "Building solution...");
		const buildResult = runCommand(
			`dotnet build ${solutionFile} --configuration Release --verbosity minimal /p:TreatWarningsAsErrors=true`,
		);
		if (!buildResult.success) {
			throw new Error("Build failed for solution");
		}

		// 4. Run tests for the main solution
		logMessage(LogLevel.INFO, "Running tests...");
		const testResult = runCommand(
			`dotnet test ${solutionFile} --configuration Release --verbosity minimal --no-build`,
		);
		if (!testResult.success) {
			throw new Error("Tests failed.");
		}

		// 5. Build documentation
		logMessage(LogLevel.INFO, "Building documentation...");
		buildDoc();

		// 6. Success
		logMessage(LogLevel.INFO, "========================================");
		logMessage(LogLevel.INFO, "✅ All checks passed successfully!");
		logMessage(LogLevel.INFO, "========================================");
	} catch (error) {
		logMessage(LogLevel.ERROR, (error as Error).message);
		process.chdir(repoRoot);
		process.exit(1);
	}
}
