# Compile the actual production selection code without opening another Unity Editor.
$source = Get-Content Assets/Scripts/Blocks/Data/BlockSelection.cs -Raw
$shim = @"
using System;
namespace UnityEngine {
 public struct Color32 { public byte r,g,b,a; }
 public struct Color {
  public byte r,g,b,a;
  public static implicit operator Color32(Color c) { return new Color32 {r=c.r,g=c.g,b=c.b,a=c.a}; }
 }
 public static class ColorUtility {
  public static bool TryParseHtmlString(string text,out Color color) {
   color=new Color(); uint n;
   if(text==null||text.Length!=7||text[0]!='#'||!uint.TryParse(text.Substring(1),System.Globalization.NumberStyles.HexNumber,System.Globalization.CultureInfo.InvariantCulture,out n))return false;
   color=new Color {r=(byte)(n>>16),g=(byte)(n>>8),b=(byte)n,a=255}; return true;
  }
 }
}
"@
Add-Type -TypeDefinition ($shim + $source.Substring($source.IndexOf('namespace Exhibition.Blocks')))
$raw = Get-Content Assets/StreamingAssets/BlockGeneration/BlockCatalog.json -Raw | ConvertFrom-Json
$catalog=[Exhibition.Blocks.BlockCatalog]::new()
$catalog.schemaVersion=$raw.schemaVersion
foreach($field in 'stylePrefix','styleSuffix','negativePrompt') {$catalog.$field=$raw.$field}
foreach($section in 'groups','products','colors','equipmentVariants') {
 $catalog.$section=@(foreach($entry in $raw.$section) {
  $option=[Exhibition.Blocks.BlockOption]::new()
  foreach($field in 'id','label','prompt','primaryPart','secondaryPart','negativePrompt','hex') {$option.$field=$entry.$field}
  if($entry.ageVariants) {
   $option.ageVariants=@(foreach($design in $entry.ageVariants) {
    $variant=[Exhibition.Blocks.BlockAgeVariant]::new()
    foreach($field in 'groupId','label','prompt','primaryPart','secondaryPart') {$variant.$field=$design.$field}
    $variant
   })
  }
  $option
 })
}
$catalog.Validate()
$selection=[Exhibition.Blocks.BlockSelection]::new()
$results=@(foreach($group in $catalog.groups) {foreach($product in $catalog.products) {foreach($first in $catalog.colors) {foreach($second in $catalog.colors) {
 $selection.Set($catalog,[Exhibition.Blocks.BlockSlot]::Group,$group.id)
 $selection.Set($catalog,[Exhibition.Blocks.BlockSlot]::Product,$product.id)
 $selection.Set($catalog,[Exhibition.Blocks.BlockSlot]::Color1,$first.id)
 $selection.Set($catalog,[Exhibition.Blocks.BlockSlot]::Color2,$second.id)
 $selection.CreatePrompt($catalog,0)
}}}})
$results | ConvertTo-Json -Depth 8 | Set-Content docs/validation/comfy_age_design/csharp_prompts.json -Encoding utf8
function Assert-Rejected([scriptblock]$action) {
 $rejected=$false
 try {& $action} catch {$rejected=$true}
 if(-not $rejected) {throw 'Invalid audience design was accepted.'}
}
$product=$catalog.products[0]
$original=$product.ageVariants
$product.ageVariants=@($original[0])
Assert-Rejected {$catalog.Validate()}
Assert-Rejected {$product.ForGroup('senior')}
$product.ageVariants=@($original[0],$original[0],$original[2],$original[3])
Assert-Rejected {$catalog.Validate()}
$product.ageVariants=$original
$original[0].groupId='unknown'
Assert-Rejected {$catalog.Validate()}
$original[0].groupId='child'
$part=$original[0].secondaryPart
$original[0].secondaryPart=''
Assert-Rejected {$catalog.Validate()}
$original[0].secondaryPart=$part
$catalog.Validate()
$selection.Remove([Exhibition.Blocks.BlockSlot]::Color1)
Assert-Rejected {$selection.CreatePrompt($catalog,0)}
$selection.Clear()
if($selection.Complete) {throw 'Selection clear failed.'}
Write-Output "Production C# offline compile: $($results.Count) prompts and six rejection paths passed. Unity ColorUtility is a hex-only test shim."
