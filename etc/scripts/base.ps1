$root = Join-Path $PSScriptRoot "../.."
$terminal = Get-Location

$solutions = @(
  Resolve-Path (Join-Path $root "Allegory.Axiom.slnx")
)