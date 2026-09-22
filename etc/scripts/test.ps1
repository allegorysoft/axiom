param(
    [Alias("c")]
    [string]$configuration = "Debug",
    [switch]$coverage,
    [string]$coverageOutput
)

. "$PSScriptRoot/base.ps1"

if (-not $coverageOutput) {
    $coverageOutput = Join-Path $root "TestResults/coverage.cobertura.xml"
}

foreach ($solution in $solutions) {

    $testArgs = @(
        "test",
        "--solution", $solution,
        "--no-build",
        "-c", $configuration,
        "--report-gh"
    )

    if ($coverage) {
        $testArgs += @(
            "--coverage",
            "--coverage-output-format", "cobertura",
            "--coverage-output", $coverageOutput
        )
    }

    dotnet @testArgs

    if (-Not $?) {
        Write-Host ("Test failed for the solution: " + $solution)
        exit $LASTEXITCODE
    }
}
