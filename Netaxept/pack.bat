@echo off
set outputDir=%~dp0

if not "%1"=="" (
  set outputDir=%1
)


nuget pack src\Geta.Epi.Commerce.Payments.Netaxept.Checkout\Geta.Epi.Commerce.Payments.Netaxept.Checkout.csproj -IncludeReferencedProjects
nuget pack src\Geta.EPi.Payments.Netaxept.CommerceManager\Geta.EPi.Payments.Netaxept.CommerceManager.csproj -IncludeReferencedProjects

@echo on