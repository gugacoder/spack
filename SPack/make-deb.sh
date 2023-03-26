#!/bin/bash

dotnet deb
mkdir -p Dist
cp bin/Debug/net6.0/*.deb Dist
