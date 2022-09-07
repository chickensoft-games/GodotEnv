
dotnet test \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat="lcov" \
  -p:CoverletOutput=./coverage/

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
mv ./badges/badge_methodcoverage.svg ./reports/method_coverage.svg

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
