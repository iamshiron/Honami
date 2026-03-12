import { execSync } from "child_process";
import { chdir } from "process";
import { join } from "path";
import { findRepoRoot } from './utils';

export function buildDoc() {
	const repoRoot = findRepoRoot();
	const docsFolder = join(repoRoot, "docs");
	const siteDir = join(docsFolder, "_site");

	console.log("Building documentation...");
	chdir(docsFolder);
	execSync("docfx metadata", { stdio: "inherit" });
	execSync(`docfx build --output ${siteDir}`, { stdio: "inherit" });
	console.log("Documentation built successfully.");

	chdir(repoRoot);
}

export function runDoc() {
	const repoRoot = findRepoRoot();
	const docsFolder = join(repoRoot, "docs");
	const siteDir = join(docsFolder, "_site");

	console.log("Running documentation...");
	chdir(docsFolder);
	execSync(`docfx serve ${siteDir}`, { stdio: "inherit" });
	console.log("Documentation is now running.");

	chdir(repoRoot);
}
