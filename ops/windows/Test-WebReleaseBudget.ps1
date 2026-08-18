[CmdletBinding()]
param([string]$BuildRoot=(Join-Path $PSScriptRoot '..\..\apps\web'),[int64]$MaximumRootJavaScriptBytes=655360,[int64]$MaximumChunkBytes=1048576,[int64]$MaximumCssBytes=262144)
$ErrorActionPreference='Stop';$nextRoot=Join-Path ([IO.Path]::GetFullPath($BuildRoot)) '.next';$manifestPath=Join-Path $nextRoot 'build-manifest.json'
if(-not(Test-Path -LiteralPath $manifestPath -PathType Leaf)){throw "Web release budget requires a completed Next.js build: $manifestPath"}
try{$manifest=Get-Content -LiteralPath $manifestPath -Raw -Encoding UTF8|ConvertFrom-Json}catch{throw "Web release manifest is invalid: $($_.Exception.Message)"}
if(-not $manifest.rootMainFiles){throw 'Web release manifest does not contain root client assets.'}
$rootBytes=0L
foreach($relativePath in @($manifest.rootMainFiles|Sort-Object -Unique)){$assetPath=Join-Path $nextRoot ($relativePath-replace '/','\');if(-not(Test-Path -LiteralPath $assetPath -PathType Leaf)){throw "Manifest asset is missing: $relativePath"};$rootBytes+=(Get-Item -LiteralPath $assetPath).Length}
$chunks=@(Get-ChildItem -LiteralPath (Join-Path $nextRoot 'static\chunks') -File -Recurse -Filter '*.js');if($chunks.Count-eq 0){throw 'Web release contains no measurable client chunks.'}
$styles=@(Get-ChildItem -LiteralPath (Join-Path $nextRoot 'static') -File -Recurse -Filter '*.css');if($styles.Count-eq 0){throw 'Web release contains no measurable stylesheets.'}
$largestChunk=($chunks|Measure-Object Length -Maximum).Maximum;$largestCss=($styles|Measure-Object Length -Maximum).Maximum
$violations=[Collections.Generic.List[string]]::new();if($rootBytes-gt $MaximumRootJavaScriptBytes){$violations.Add('root-javascript')};if($largestChunk-gt $MaximumChunkBytes){$violations.Add('client-chunk')};if($largestCss-gt $MaximumCssBytes){$violations.Add('stylesheet')}
[pscustomobject]@{CheckedAt=[DateTimeOffset]::UtcNow.ToString('o');RootJavaScriptBytes=$rootBytes;MaximumRootJavaScriptBytes=$MaximumRootJavaScriptBytes;LargestChunkBytes=$largestChunk;MaximumChunkBytes=$MaximumChunkBytes;LargestCssBytes=$largestCss;MaximumCssBytes=$MaximumCssBytes;Passed=$violations.Count-eq 0;Violations=$violations}|ConvertTo-Json -Depth 3
if($violations.Count-gt 0){exit 1};exit 0
