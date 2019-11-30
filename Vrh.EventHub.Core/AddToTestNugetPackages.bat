@echo off
REM A bin\Release-ben vagy bin\Debug-ban létrejött NuGet csomag hozzáadása a helyi teszt NuGet csomag mappához.
REM 1. par: A verziószám
REM 2. par: A project mappája
REM 3. par: A konfiguráció neve (Debug vagy Release)
REM 4. par: A NuGet csomag neve, ami általában a solution név. !! Ha mégsem, akkor a Build events-ben javítani kell !
SETLOCAL ENABLEEXTENSIONS
SET me=%~n0
ECHO %me%: START
SET projectversion=1.2.1
SET nuexe=.\_CreateNewNuGetPackage\DoNotModify\NuGet
SET nuname=Vrh.EventHub.Core
SET nupkg=.\bin\Release\%nuname%.%projectversion%.nupkg
SET packfolder=D:\Test NuGet Packages
IF EXIST "%packfolder%" (
	IF EXIST "%nupkg%" (
		"%nuexe%" delete "%nuname%" "%projectversion%" -source "%packfolder%" -NonInteractive
		"%nuexe%" add "%nupkg%" -source "%packfolder%"
	) ELSE (
		ECHO %me%: NuGet package not found! Package: "%nupkg%"
	)
) ELSE (
	ECHO %me%: NuGet package destination folder does not exist!
)
ECHO %me%: END