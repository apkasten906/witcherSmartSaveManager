# Run-PesterTests.ps1
# Script to automate the execution of Pester tests and generate a report

# Define the path to the Tests folder
$testsPath = "C:\Development\witcherSmartSaveManager\Tests"

# Define the output file for the test results
$outputFile = "$testsPath\TestResults.xml"

# Run Pester tests and generate an NUnit XML report
Invoke-Pester -Path $testsPath -OutputFormat NUnitXml -OutputFile $outputFile

# Display a message with the location of the test results
Write-Host "Pester tests completed. Results saved to: $outputFile" -ForegroundColor Green
