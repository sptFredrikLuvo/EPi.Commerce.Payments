@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)


nuget pack Geta.EPi.Commerce.Payments.Klarna.Checkout.Manager.nuspec -IncludeReferencedProjects
nuget pack Geta.EPi.Commerce.Payments.Klarna.Checkout.csproj -IncludeReferencedProjects

@echo on