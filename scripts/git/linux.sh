#!/bin/bash

# Check if pnpm or npm is installed
if command -v pnpm &> /dev/null
then
    pnpm gitHook
elif command -v npm &> /dev/null
then
    npm run gitHook
else
    echo "Neither pnpm nor npm is installed. Please install one of them to proceed."
fi
