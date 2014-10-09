@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)

nuget pack -IncludeReferencedProjects -OutputDirectory %outputDir%

@echo on