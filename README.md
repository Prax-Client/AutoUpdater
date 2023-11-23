# AutoUpdater

## Overview

**Name:** AutoUpdater  
**Purpose:** Helps update your cheat offsets for Minecraft Bedrock Edition (MCBE).

## Table of Contents

1. [Introduction](#introduction)
2. [Features](#features)
3. [Requirements](#requirements)
4. [Installation](#installation)
5. [Configuration](#configuration)
6. [Usage](#usage)
7. [Contributing](#contributing)
   
## Introduction

AutoUpdater is a C# project designed to simplify the process of updating cheat offsets for Minecraft Bedrock Edition (MCBE). It automates the download of the latest server executable, analyzes it, and updates offsets using a configuration file.

## Features

- **Automatic Updates:** Automatically downloads the latest MCBE Windows server executable.
- **Offset Analysis:** Analyzes the downloaded server executable to identify and update offsets.
- **Configuration File:** Utilizes a configuration file to specify and update offsets according to the user's needs.

## Requirements

- **Operating System:** Windows
- **LLVM:** [MSYS2](https://www.msys2.org/) (Contains name demangler and allows for public and section dumping) (Contains name demangler and allows for public and section dumping)
## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/Prax-Client/AutoUpdater.git
   ```

2. Open the solution in your preferred C# development environment.

3. Build the solution to generate the executable.

## Configuration

AutoUpdater uses a configuration file (`config.json`) to manage offsets. The configuration file has the following structure:

```json
{
  "WorkingDirectory": "D:\\Source\\AutoUpdate\\AutoUpdate\\bin\\Release\\net7.0\\win-x64\\publish",
  "PdbFile": "D:\\Source\\AutoUpdate\\AutoUpdate\\bin\\Release\\net7.0\\win-x64\\publish\\bedrock-server-1.20.40.01\\bedrock_server.pdb",
  "ExeFile": "D:\\Source\\AutoUpdate\\AutoUpdate\\bin\\Release\\net7.0\\win-x64\\publish\\bedrock-server-1.20.40.01\\bedrock_server.exe",
  "LlvmInstallDirectory": "C:\\msys64\\mingw64\\bin",
  "PdbUtil": "C:\\msys64\\mingw64\\bin\\llvm-pdbutil.exe",
  "DemangleUtil": "C:\\msys64\\mingw64\\bin\\llvm-undname.exe",
  "UpdateItems": [
    {
      "Name": "offset1",
      "Function": "?getSupplies@Player@@QEBAAEBVPlayerInventory@@XZ",
      "Resolver": {
        "Type": 1,
        "Value": "3"
      }
    },
    {
      "Name": "offset2",
      "Function": "?getSupplies@Player@@QEBAAEBVPlayerInventory@@XZ",
      "Resolver": {
        "Type": 0,
        "Value": "? ? ? ? C3"
      }
    }
  ]
}
```

Replace `"offset1"`, `"offset2"`, etc., with the specific offsets you want to update and their corresponding values.

## Usage

1. Run the AutoUpdater executable.

2. The tool will prompt to download the latest MCBE Windows server executable.

3. It will then analyze the downloaded executable to identify offsets.

4. Update offsets according to the configuration file.

5. The updated offsets can now be used with your cheat.

## Contributing

Contributions are welcome! If you find a bug or have an enhancement in mind, please open an issue or submit a pull request.
