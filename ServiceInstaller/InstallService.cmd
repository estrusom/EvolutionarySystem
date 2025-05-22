@echo off
cls
echo Start Install SemanticProcessor Windows Service
setlocal
cd /d %~dp0
echo. 

rem check is admin
net.exe session 1>NUL 2>NUL || goto is_not_admin

<nul set /p=Unblocked files 
FOR %%a in (*.*) do (
rem	sleep 0.2
	<nul set /p=.
	echo.>%%a:Zone.Identifier
	)
echo.
set uninst=0
IF .%1. == ./u. (set "uninst=1")
IF .%1. == ./U. (set "uninst=1")
if %uninst% == 1 (
	echo Uninstall SemanticProcessor Windows Service ...
	net stop SemanticProcessor > nul
	)
if %uninst% == 0 (echo Install SemanticProcessor Windows Service ...)


InstallUtil.exe %1 SemanticProcessor.exe
if %ERRORLEVEL%==0 goto wintab_config
pause
goto installutil_error

:wintab_config
echo.  
if %uninst% == 1 (goto end)
echo Configure SemanticProcessor Windows Service ...
sc config SemanticProcessor start= auto
sc failure SemanticProcessor reset=5 actions=restart/15000/run/15000
goto end

:is_not_admin
echo.  
Echo This script requires elevated rights. Run with admin user.
pause
exit /b 1

:installutil_error
echo.  
Echo Error %ERRORLEVEL% to execute InstallUtil.exe
pause
exit /b 1


:end
echo.  
if %uninst% == 1 (echo SemanticProcessor Service is uninstalled)
if %uninst% == 0 (
	echo SemanticProcessor Service is installed
	net start SemanticProcessor
	)
pause