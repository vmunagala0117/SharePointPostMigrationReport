#Assuming C:\Logs exists and all your discrepency reports are generated in the subweb level folders. 
#Navigate to the relevant site collection folder and then run it: For ex: C:\Logs\<site collection folder>
$missingListItemsCsv = Get-ChildItem -Include "missingListItems.csv" -Recurse
foreach($csv in $missingListItemsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\MissingListItems.csv" -Append -NoTypeInformation
 write-host $csv 
}

$listItemsCountMismatchCsv = Get-ChildItem -Include "listItemsCountMismatch.csv" -Recurse
foreach($csv in $listItemsCountMismatchCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\ListItemsCountMismatch.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingFieldsCsv = Get-ChildItem -Include "missingFields.csv" -Recurse
foreach($csv in $missingFieldsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object * | Export-Csv -LiteralPath "C:\Logs\MissingFields.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingListsCsv = Get-ChildItem -Include "missingLists.csv" -Recurse
foreach($csv in $missingListsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv -Header Name | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\MissingLists.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingWebGroupsCsv = Get-ChildItem -Include "missingWebGroups.csv" -Recurse
foreach($csv in $missingWebGroupsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv -Header Name | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\MissingWebGroups.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingSiteGroupsCsv = Get-ChildItem -Include "missingSiteGroups.csv" -Recurse
foreach($csv in $missingSiteGroupsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv -Header Name | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\MissingSiteGroups.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingWPsCsv = Get-ChildItem -Include "missingWebParts.csv" -Recurse
foreach($csv in $missingWPsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object * | Export-Csv -LiteralPath "C:\Logs\MissingWebParts.csv" -Append -NoTypeInformation
 write-host $csv 
}


$missingListViewsCsv = Get-ChildItem -Include "missingListViews.csv" -Recurse
foreach($csv in $missingListViewsCsv) {

$path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\MissingListViews.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingListsCsv = Get-ChildItem -Include "missingLists.csv" -Recurse
foreach($csv in $missingListsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv -Header Name | Select-Object *, @{Name='Url';Expression={$path}} | Export-Csv -LiteralPath "C:\Logs\MissingLists.csv" -Append -NoTypeInformation
 write-host $csv 
}

$mismatchWebPermsInheritanceCsv = Get-ChildItem -Include "mismatchWebPermsInheritance.csv" -Recurse
foreach($csv in $mismatchWebPermsInheritanceCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object * | Export-Csv -LiteralPath "C:\Logs\MismatchWebPermsInheritance.csv" -Append -NoTypeInformation
 write-host $csv 
}

$missingWorkflowsCsv = Get-ChildItem -Include "missingWorkflows.csv" -Recurse
foreach($csv in $missingWorkflowsCsv) {

 $path = $csv.DirectoryName -replace [regex]::Escape("C:\Logs"), ""
 Import-Csv $csv | Select-Object * | Export-Csv -LiteralPath "C:\Logs\MissingWorkflows.csv" -Append -NoTypeInformation
 write-host $csv 
}