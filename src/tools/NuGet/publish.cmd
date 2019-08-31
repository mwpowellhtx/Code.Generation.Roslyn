@echo off

setlocal

rem Which is set in the Environment Variables for security reasons.
set nuget_api_key=%MY_NUGET_API_KEY%
set nuget_api_src=https://api.nuget.org/v3/index.json
rem Set debug=1 to Debug for any reason.
set debug=false

rem Default list delimiter is Comma (,).
:redefine_delim
if "%delim%" == "" (
    set delim=,
)
rem Redefine the delimiter when a Dot (.) is discovered.
rem Anticipates potentially accepting Delimiter as a command line arg.
if "%delim%" == "." (
    set delim=
    goto :redefine_delim
)

rem Go ahead and pre-seed the Projects up front.
:set_projects
set projects=
rem Setup All Projects
set all_projects=Code.Generation.Roslyn
set all_projects=%all_projects%%delim%Code.Generation.Roslyn.Engine
set all_projects=%all_projects%%delim%Code.Generation.Roslyn.Attributes
set all_projects=%all_projects%%delim%Code.Generation.Roslyn.Tool
set all_projects=%all_projects%%delim%Code.Generation.Roslyn.Tasks
rem Setup Roslyn Projects
set roslyn_projects=Code.Generation.Roslyn
rem Setup Engine Projects
set engine_projects=Code.Generation.Roslyn.Engine
set engine_projects=%engine_projects%%delim%Code.Generation.Roslyn
set engine_projects=%engine_projects%%delim%Code.Generation.Roslyn.Attributes
rem Setup Attributes Projects
set attrib_projects=Code.Generation.Roslyn.Attributes
rem Setup `dotnet´ Tooling Projects
set tool_projects=Code.Generation.Roslyn.Tool
rem Setup Tasks Projects
set tasks_projects=Code.Generation.Roslyn.Tasks

:parse_args
:next_arg

:set_debug
if "%1"=="--debug" (
  set debug=true
  goto :fini_arg
)

:set_destination
if "%1"=="--local" (
  set destination=local
  goto :fini_arg
)
if "%1"=="--nuget" (
  set destination=nuget
  goto :fini_arg
)

:set_dry_run
if "%1" == "--dry" (
    set dry=true
    goto :fini_arg
)
if "%1" == "--dry-run" (
    set dry=true
    goto :fini_arg
)
if "%1" == "--wet" (
    set dry=false
    goto :fini_arg
)
if "%1" == "--wet-run" (
    set dry=false
    goto :fini_arg
)

:set_config
if "%1"=="--config" (
  set config=%2
  shift
  goto :fini_arg
)

:add_roslyn_projects
if "%1" == "--roslyn" (
    if "%projects%" == "" (
        set projects=%roslyn_projects%
    ) else (
        set projects=%projects%%delim%%roslyn_projects%
    )
    goto :fini_arg
)

:add_engine_projects
if "%1" == "--engine" (
    if "%projects%" == "" (
        set projects=%engine_projects%
    ) else (
        set projects=%projects%%delim%%engine_projects%
    )
    goto :fini_arg
)

:add_attrib_projects
if "%1" == "--attrib" (
    if "%projects%" == "" (
        set projects=%attrib_projects%
    ) else (
        set projects=%projects%%delim%%attrib_projects%
    )
    goto :fini_arg
)

:add_tool_projects
if "%1" == "--tool" (
    if "%projects%" == "" (
        set projects=%tool_projects%
    ) else (
        set projects=%projects%%delim%%tool_projects%
    )
    goto :fini_arg
)

:add_tasks_projects
if "%1" == "--tasks" (
    if "%projects%" == "" (
        set projects=%tasks_projects%
    ) else (
        set projects=%projects%%delim%%tasks_projects%
    )
    goto :fini_arg
)

:add_all_projects
if "%1" == "--all" (
    rem Prepare to publish All Projects.
    set projects=%all_projects%
    goto :fini_arg
)

:add_project
if "%1" == "--project" (
    rem Add a Project to the Projects list.
    if "%projects%" == "" (
        set projects=%2
    ) else (
        set projects=%projects%%delim%%2
    )
    shift
    goto :fini_arg
)

:fini_arg

shift

if "%1"=="" goto :end_args

goto :next_arg

:end_args

:verify_args

:verify_dry
rem Assumes we want a Live (Wet) Run when unspecified.
if "%dry%" == "" set dry=false

:verify_destination
rem Do not accidentally Publish anything to NuGet.
if "%destination%" == "" set destination=local

rem A NuGet API Key is required when the Destination is NuGet (lowercase).
if "%destination%%nuget_api_key%"=="nuget" (
  echo "NuGet API Key required in your 'MY_NUGET_API_KEY' Environment Variable."
  goto :fini
)

:verify_config
rem Assumes Release Configuration when not otherwise specified.
if "%config%" == "" set config=Release

:eval_publications

if "%debug%"=="true" (
  echo config=%config%
  echo destination=%destination%
  echo nuget_api_key=%nuget_api_key%
  echo nuget_api_src=%nuget_api_src%
  echo publish_local_dir=%publish_local_dir%
  goto :fini
)

set nupkg_ext=.nupkg

set xcopy_exe=xcopy.exe
set publish_local_dir=G:\Dev\NuGet\local\packages

set nuget_exe=nuget.exe
set nuget_push_verbosity=detailed

set nuget_push_opts=%nuget_push_opts% %nuget_api_key%
set nuget_push_opts=%nuget_push_opts% -Verbosity %nuget_push_verbosity%
set nuget_push_opts=%nuget_push_opts% -NonInteractive
set nuget_push_opts=%nuget_push_opts% -Source %nuget_api_src%

rem Do the main areas here.
pushd ..\..

if "%debug%" == "true" (
    if not "%projects%" == "" (
        echo Processing '%config%' configuration for '%projects%' ...
    )
)
:next_project
if not "%projects%" == "" (
    for /f "tokens=1* delims=%delim%" %%p in ("%projects%") do (
        call :publish_pkg %%p
        set projects=%%q
        goto :next_project
    )
)

popd

goto :fini

:publish_pkg
set package_name=%1
rem These are a special cases...
if "%package_name%" == "Code.Generation.Roslyn.Tasks" set package_name=Code.Generation.Roslyn.BuildTime
if "%package_name%" == "Code.Generation.Roslyn.Tool" set package_name=dotnet-cgr
set publish_pattern=%1\bin\%config%\%package_name%*%nupkg_ext%
if not exist "%publish_pattern%" (
    echo Package '%publish_pattern%' not found.
)
for %%f in ("%publish_pattern%") do (

    if "%destination%-%dry%" == "local-true" (
        echo Set to copy "%%f" to "%publish_local_dir%".
    )
    if "%destination%-%dry%" == "local-false" (
        if not exist "%publish_local_dir%" mkdir "%publish_local_dir%"
        echo Copying "%%f" package to local directory "%publish_local_dir%" ...
        %xcopy_exe% /q /y "%%f" "%publish_local_dir%"
    )

    if "%destination%-%dry%" == "nuget-true" (
        echo Dry run: %nuget_exe% push "%%f"%nuget_push_opts%
    )
    if "%destination%-%dry%" == "nuget-false" (
        echo Running: %nuget_exe% push "%%f"%nuget_push_opts%
        %nuget_exe% push "%%f"%nuget_push_opts%
    )
)
exit /b

:fini

endlocal
