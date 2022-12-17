#!/bin/bash

dotnet build --no-restore

# This requires a GODOT4 environment variable.

dotnet ~/coverlet/src/coverlet.console/bin/Debug/net5.0/coverlet.console.dll \
  "./.godot/mono/temp/bin/Debug" --verbosity detailed \
  --target $GODOT4 \
  --targetargs "--run-tests --coverage --quit-on-finish" \
  --format "opencover" \
  --output "./coverage/coverage.xml" \
  --exclude-by-file "**/test/**/*.cs" \
  --exclude-by-file "**/*Microsoft.NET.Test.Sdk.Program.cs" \
  --exclude-assemblies-without-sources "missingall"

# we filter out local project references with -assemblyfilters

reportgenerator \
  -reports:"./coverage/coverage.xml" \
  -targetdir:"./coverage/report" \
  "-assemblyfilters:-GoDotCollections;-GoDotLog" \
  -reporttypes:"Html"

reportgenerator \
  -reports:"./coverage/coverage.xml" \
  -targetdir:"./badges" \
  "-assemblyfilters:-GoDotCollections;-GoDotLog" \
  -reporttypes:"Badges"

mv ./badges/badge_branchcoverage.svg ./reports/branch_coverage.svg
mv ./badges/badge_linecoverage.svg ./reports/line_coverage.svg

rm -rf ./badges

# Determine OS, open coverage accordingly.

case "$(uname -s)" in

   Darwin)
     echo 'Mac OS X'
     open coverage/report/index.htm
     ;;

   Linux)
     echo 'Linux'
     ;;

   CYGWIN*|MINGW32*|MSYS*|MINGW*)
     echo 'MS Windows'
     start coverage/report/index.htm
     ;;

   *)
     echo 'Other OS'
     ;;
esac
