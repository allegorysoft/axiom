param(
    [Alias("c")]
    [string]$configuration = "Debug",
    [switch]$Coverage,
    [string]$CoverageOutput
)

. "$PSScriptRoot/base.ps1"

if (-not $CoverageOutput) {
    $CoverageOutput = Join-Path $root "TestResults/coverage.cobertura.xml"
}

foreach ($solution in $solutions) {

    $testArgs = @(
        "test",
        "--solution", $solution,
        "--no-build",
        "-c", $configuration,
        "--report-gh"
    )

    if ($Coverage) {
        $testArgs += @(
            "--coverage",
            "--coverage-output-format", "cobertura",
            "--coverage-output", $CoverageOutput
        )
    }

    dotnet @testArgs

    if (-Not $?) {
        Write-Host ("Test failed for the solution: " + $solution)
        exit $LASTEXITCODE
    }
}
