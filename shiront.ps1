#!/usr/bin/env pwsh

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$scriptsPath = Join-Path $scriptDir "scripts"

Push-Location $scriptsPath
pnpm tools @args
Pop-Location
