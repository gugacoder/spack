#!/bin/bash

cd SPack

mkdir -p ../Dist

echo
echo "[INFO] ------------------------------------------------------------------"
echo "[INFO] Cleaning"
echo "[INFO] ------------------------------------------------------------------"
echo
dotnet clean

echo
echo "[INFO] ------------------------------------------------------------------"
echo "[INFO] Publishing single command line application for linux-x64"
echo "[INFO] ------------------------------------------------------------------"
echo
dotnet publish -r linux-x64 -c Release -o ../Dist /p:DebugType=none /p:PublishSingleFile=true

echo
echo "[INFO] ------------------------------------------------------------------"
echo "[INFO] Publishing DEB package for linux-x64"
echo "[INFO] ------------------------------------------------------------------"
echo
dotnet deb -r linux-x64 -c Release -o ../Dist

echo
echo "[INFO] ------------------------------------------------------------------"
echo "[INFO] Publishing single command line application for win-x64"
echo "[INFO] ------------------------------------------------------------------"
echo
dotnet publish -r win-x64 -c Release -o ../Dist /p:DebugType=none /p:PublishSingleFile=true

# Removendo as bibliotecas nativas da pasta de distribuição.
# Estas bibliotecas já são embutidas no executável a partir da pasta META-INFO/runtimes.
if ls ../Dist/*.dll 1> /dev/null 2>&1; then
  rm ../Dist/*.dll
fi

