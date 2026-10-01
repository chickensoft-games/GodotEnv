#!/bin/bash

dotnet build

TESTINGPLATFORM_TELEMETRY_OPTOUT=1 dotnet test \
  --results-directory "./coverage" \
  --coverlet \
  --coverlet-output-format opencover \
  --coverlet-include "[Chickensoft.GodotEnv*]*"

mv ./coverage/coverage.opencover.*.xml ./coverage/coverage.opencover.xml

reportgenerator \
  -reports:"./coverage/coverage.opencover.xml" \
  -targetdir:"./coverage/report" \
  -reporttypes:"Html;Badges"

mkdir -p ./reports

mv ./coverage/report/badge_branchcoverage.svg ./reports/branch_coverage.svg
mv ./coverage/report/badge_linecoverage.svg ./reports/line_coverage.svg
mv ./coverage/report/badge_methodcoverage.svg ./reports/method_coverage.svg

# Determine OS, open coverage accordingly.

case "$(uname -s)" in

   Darwin)
      echo 'Mac OS X'
      open coverage/report/index.htm
     ;;

   Linux)
      echo 'Linux'
      xdg-open coverage/report/index.htm
     ;;

   CYGWIN*|MINGW32*|MSYS*|MINGW*)
      echo 'MS Windows'
      start coverage/report/index.htm
     ;;

   *)
      echo 'Other OS'
      ;;
esac
