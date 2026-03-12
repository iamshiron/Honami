import FS from "node:fs";
import Path from "node:path";
import { findRepoRoot } from './utils';

// Configuration: Root directory to scan (defaults to current working directory)
const ROOT_DIR: string = findRepoRoot();

// 1. Recursive function to find all .csproj files
function findAllCsproj(dir: string, fileList: string[] = []): string[] {
	const files = FS.readdirSync(dir);

	files.forEach((file) => {
		const filePath = Path.join(dir, file);
		const stat = FS.statSync(filePath);

		if (stat.isDirectory()) {
			if (file !== "obj" && file !== "bin" && !file.startsWith(".")) {
				// Skip build folders
				findAllCsproj(filePath, fileList);
			}
		} else {
			if (Path.extname(file) === ".csproj") {
				fileList.push(filePath);
			}
		}
	});

	return fileList;
}

// Interface for parsed project data
interface Project {
	name: string;
	refs: string[];
}

// 2. Parse a single .csproj file to find ProjectReferences
function parseCsproj(filePath: string): Project {
	const content = FS.readFileSync(filePath, "utf8");
	const projectName = Path.basename(filePath, ".csproj");
	const references: string[] = [];

	// Simple regex to match <ProjectReference Include="..." />
	// Matches both standard include and self-closing tags
	const regex = /<ProjectReference\s+Include="([^"]+)"/g;
	let match: RegExpExecArray | null = regex.exec(content);
	while (match !== null) {
		// match[1] is the relative path, e.g., "..\\..\\Phonon\\Engine\\Phonon.Engine.csproj"
		// We only care about the filename at the end
		const refPath = match[1];
		const refName = Path.basename(refPath, ".csproj");
		// Convert backward slashes to forward slashes just in case
		references.push(refName);
		match = regex.exec(content);
	}

	return { name: projectName, refs: references };
}

// 3. Main Logic
function generateMermaid(): void {
	const files = findAllCsproj(ROOT_DIR);
	const projects: Project[] = files.map(parseCsproj);

	console.log("```mermaid");
	console.log("graph TD");
	console.log("    %% Nodes");

	// Print Nodes (and apply basic styling classes based on naming convention)
	projects.forEach((p) => {
		let style = "";
		// Optional: Add some color based on your specific layers
		if (p.name.includes("Runtime")) style = ":::exe";
		else if (p.name.includes("Engine")) style = ":::engine";
		else if (p.name.includes("Common")) style = ":::common";

		console.log(`    ${p.name.replace(/\./g, "_")}["${p.name}"]${style}`);
	});

	console.log("\n    %% Relationships");
	projects.forEach((p) => {
		p.refs.forEach((ref) => {
			// Replace dots with underscores for valid Mermaid IDs
			const from = p.name.replace(/\./g, "_");
			const to = ref.replace(/\./g, "_");
			console.log(`    ${from} --> ${to}`);
		});
	});

	// Add styles
	console.log("\n    %% Styles");
	console.log(
		"    classDef exe fill:#f96,stroke:#333,stroke-width:2px,color:black;",
	);
	console.log(
		"    classDef engine fill:#9cf,stroke:#333,stroke-width:2px,color:black;",
	);
	console.log(
		"    classDef common fill:#cfc,stroke:#333,stroke-width:2px,color:black;",
	);
	console.log("```");
}

export function runGraph() {
	generateMermaid();
}

export { generateMermaid };
