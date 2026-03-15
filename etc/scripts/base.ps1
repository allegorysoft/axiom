$root = Join-Path $PSScriptRoot "../.."
$terminal = Get-Location

$solutions = @(
  Resolve-Path (Join-Path $root "/framework/Allegory.Axiom.slnx")
)