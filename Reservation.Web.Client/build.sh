#!/usr/bin/env bash
set -o errexit

DOTNET_DOWNLOAD_URL="https://download.visualstudio.microsoft.com/download/pr/9d07577e-f7bc-4d60-838d-f79c50b5c11a/459ef339396783db369e0432d6dc3d7e/dotnet-sdk-8.0.407-linux-x64.tar.gz"

echo "Stahuji .NET SDK..."
curl -SL $DOTNET_DOWNLOAD_URL -o dotnet-sdk.tar.gz

echo "Extrahuji SDK..."
mkdir -p $HOME/dotnet
tar -zxf dotnet-sdk.tar.gz -C $HOME/dotnet

export DOTNET_ROOT=$HOME/dotnet
export PATH=$PATH:$HOME/dotnet

echo "Verifikuji instalaci..."
dotnet --version

echo "Spouštím dotnet publish..."
dotnet publish -c Release -o build
