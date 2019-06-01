@echo off

setlocal

rem Which is set in the Environment Variables for security reasons.
set nuget_api_key=%MY_NUGET_API_KEY%
set nuget_api_src=https://api.nuget.org/v3/index.json
set publish_local_dir=G:\Dev\NuGet\local\packages
set nuget_exe=nuget.exe
set xcopy_exe=xcopy.exe
rem Set debug=1 to Debug for any reason.
set debug=

:parse_args
:next_arg

if "%1"=="--local" (
  set destination=local
  goto :fini_arg
)

if "%1"=="--nuget" (
  set destination=nuget
  goto :fini_arg
)

if "%1"=="--config" (
  shift
  goto :fini_arg
)

:fini_arg

shift

if "%1"=="" goto :end_args

goto :next_arg

:end_args

:verify_args
rem Do not accidentally Publish anything to NuGet.
if "%destination%"=="" set destination=local

rem A NuGet API Key is required when the Destination is NuGet (lowercase).
if "%destination%%nuget_api_key%"=="nuget" (
  echo "NuGet API Key required in your 'MY_NUGET_API_KEY' Environment Variable."
  goto :fini
)

rem Assumes Release Configuration by default.
if "%config%"=="" set config=Release

if "%debug%"=="1" (
  echo config=%config%
  echo destination=%destination%
  echo nuget_api_key=%nuget_api_key%
  echo nuget_api_src=%nuget_api_src%
  echo publish_local_dir=%publish_local_dir%
  goto :fini
)

call :run_publish Kingdom.Combinatorics.Combinatorials

goto :fini

:run_publish
rem Basically drawing from any available (built) package in the appropriate configuration.
set push_dir=..\..\%1\bin\%config%
echo Pushing directory "%push_dir%" ...
pushd %push_dir%
for %%n in (*.nupkg) do (
  if "%destination%"=="local" (
    echo Publishing "%%n" to "%publish_local_dir%" ...
    %xcopy_exe% /q /y "%%n" "%publish_local_dir%"
  )
  if "%destination%"=="nuget" (
    echo Publishing "%%n" to NuGet...
	%nuget_exe% push "%%n" %nuget_api_key% -Source %nuget_api_src%
  )
)
echo Popping directory...
popd
exit /b 0

:fini

endlocal
