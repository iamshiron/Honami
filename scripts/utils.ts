import { execSync } from "node:child_process";
import FS from "node:fs";
import path from 'node:path';

export function scanForSlnFile(dir: string) {
	const files = FS.readdirSync(dir);
	for (const file of files) {
		if (file.endsWith(".sln") || file.endsWith(".slnx")) {
			return file;
		}
	}
	return null;
}

export function findRepoRoot() {
    if(FS.existsSync(".git")) {
        return process.cwd()
    }

    if(FS.existsSync("../.git")) {
        return path.join(process.cwd(), "..")
    }

    throw new Error("Could not determine repo root!")
}

export interface CommandResult {
	success: boolean;
	errorMessage?: string;
}

export function runCommand(command: string): CommandResult {
	try {
		execSync(command, { stdio: "inherit" });
		return { success: true };
	} catch {
		return { success: false, errorMessage: `Command failed: ${command}` };
	}
}
