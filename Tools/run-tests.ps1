$ErrorActionPreference = "Stop"

$requiredUnityVersion = "6000.3.16f1"
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot ".."))
$resultsDirectory = Join-Path $projectRoot "TestResults"
$resultsPath = Join-Path $resultsDirectory "editmode-results.xml"
$logPath = Join-Path $resultsDirectory "editmode.log"

if (-not [string]::IsNullOrWhiteSpace($env:UNITY_PATH)) {
    $unityPath = $env:UNITY_PATH.Trim('"')
    $unitySource = "UNITY_PATH"
}
else {
    $unityPath = Join-Path $env:ProgramFiles "Unity\Hub\Editor\$requiredUnityVersion\Editor\Unity.exe"
    $unitySource = "the Unity Hub installation for $requiredUnityVersion"
}

if (-not (Test-Path -LiteralPath $unityPath -PathType Leaf)) {
    Write-Error "Unity $requiredUnityVersion was not found via $unitySource at '$unityPath'. Set UNITY_PATH to the full path of that version's Unity.exe."
    exit 1
}

$unityProductVersion = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($unityPath).ProductVersion
$detectedUnityVersion = ($unityProductVersion -split "_")[0]
if ($detectedUnityVersion -ne $requiredUnityVersion) {
    Write-Error "Unity at '$unityPath' is version '$detectedUnityVersion'; this project requires exactly '$requiredUnityVersion'. Set UNITY_PATH to the correct Unity.exe."
    exit 1
}

New-Item -ItemType Directory -Path $resultsDirectory -Force | Out-Null
Remove-Item -LiteralPath $resultsPath, $logPath -Force -ErrorAction SilentlyContinue

$unityArguments = @(
    "-batchmode"
    "-projectPath", "`"$projectRoot`""
    "-runTests"
    "-testPlatform", "EditMode"
    "-testResults", "`"$resultsPath`""
    "-logFile", "`"$logPath`""
)

# The test runner exits Unity when the run finishes. Passing -quit here makes
# Unity 6000.3.16f1 quit during startup before Edit Mode tests are scheduled.

Write-Host "Running Unity $requiredUnityVersion Edit Mode tests..."
Write-Host "Project: $projectRoot"
Write-Host "Results: $resultsPath"
Write-Host "Log: $logPath"

try {
    $unityProcess = Start-Process -FilePath $unityPath -ArgumentList $unityArguments -Wait -PassThru -WindowStyle Hidden
    $unityExitCode = $unityProcess.ExitCode
}
catch {
    Write-Error "Unity failed to start: $($_.Exception.Message)"
    exit 1
}

if (-not (Test-Path -LiteralPath $resultsPath -PathType Leaf)) {
    Write-Error "Unity exited with code $unityExitCode and did not produce the expected test result file. See '$logPath'."
    exit 1
}

try {
    [xml]$testResults = Get-Content -LiteralPath $resultsPath -Raw
    $testRun = $testResults.'test-run'
    $total = [int]$testRun.total
    $passed = [int]$testRun.passed
    $failed = [int]$testRun.failed
}
catch {
    Write-Error "The test result file could not be parsed: $($_.Exception.Message). See '$resultsPath' and '$logPath'."
    exit 1
}

if ($unityExitCode -ne 0) {
    Write-Error "Unity exited with code $unityExitCode. Test summary: $passed passed, $failed failed, $total total. See '$resultsPath' and '$logPath'."
    exit $unityExitCode
}

if ($testRun.result -ne "Passed" -or $failed -ne 0 -or $total -eq 0) {
    Write-Error "Edit Mode tests failed or no tests ran: $passed passed, $failed failed, $total total. See '$resultsPath' and '$logPath'."
    exit 1
}

Write-Host "Edit Mode tests passed: $passed passed, $total total."
exit 0
