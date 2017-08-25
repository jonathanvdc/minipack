#!/bin/bash

# This script downloads ecsc and builds it locally.
if [ ! -d ecsc ]; then
    git clone --depth=5 https://github.com/jonathanvdc/ecsc
fi
nuget restore ecsc/src/ecsc.sln
msbuild /p:Configuration=Release /verbosity:quiet ecsc/src/ecsc.sln
