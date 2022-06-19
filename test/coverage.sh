#!/bin/bash
dotnet build

# Only collect coverage for go_dot_test addon:
coverlet .mono/temp/bin/Debug/ --target $GODOT --targetargs \
  "--run-tests --quit-on-finish" --format "lcov" \
  --output ./coverage/coverage.info \
  --exclude-by-file "**/scenes/**/*.cs" \
  --exclude-by-file "**/test/**/*.cs"

reportgenerator \
  -reports:"./coverage/coverage.info" \
  -targetdir:"./coverage/report" \
  -reporttypes:Html

reportgenerator \
  -reports:"./coverage/coverage.info" \
  -targetdir:"./badges" \
  -reporttypes:Badges

mv ./badges/badge_branchcoverage.svg ./reports/branch_coverage.svg
mv ./badges/badge_linecoverage.svg ./reports/line_coverage.svg

rm -rf ./badges
