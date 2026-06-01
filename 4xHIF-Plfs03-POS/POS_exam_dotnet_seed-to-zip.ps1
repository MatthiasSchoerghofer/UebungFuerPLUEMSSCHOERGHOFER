<#
.SYNOPSIS
    A script to create a zip file of the project seed
.DESCRIPTION
    A script to create a zip file of the project seed
.NOTES
    File Name      : POS_exam_dotnet_seed-to-zip.ps1
#>

$zipFile = "POS_exam_dotnet_seed.zip"
$nugetConfig = "nuget.config"

# Remove existing zip if present
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

# Copy config
Copy-Item "nuget.config.intern" $tempConfig -Verbose

cp -v nuget.config.intern nuget.config
zip -v -r POS_exam_dotnet_seed.zip . -x @zip-exclude.lst
del nuget.config




# Create zip archive
Compress-Archive -Path * -DestinationPath $zipFile -Force

# Cleanup temp file
Remove-Item $tempConfig -Force