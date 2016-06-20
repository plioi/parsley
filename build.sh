#!/bin/bash
dotnet restore --verbosity Warning src/Build
dotnet run --project src/Build -- $*