
dotnet test \
  -p:CollectCoverage=true \
  -p:CoverletOutputFormat="lcov" \
  -p:CoverletOutput=./coverage/

reportgenerator \
  -reports:"./coverage/coverage.info" \
  -targetdir:"./coverage/report" \
  -reporttypes:Html

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
