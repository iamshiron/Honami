# Check if pnpm or npm is installed
if (Get-Command pnpm -ErrorAction SilentlyContinue) {
    pnpm run tools gitHook
} elseif (Get-Command npm -ErrorAction SilentlyContinue) {
    npm run tools gitHook
} else {
    Write-Host "Neither pnpm nor npm is installed. Please install one of them to proceed."
}
