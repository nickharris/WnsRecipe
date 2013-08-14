param($installPath, $toolsPath, $package, $project)

$path = [System.IO.Path]
$readmeFile = "https://github.com/nickharris/WnsRecipe/blob/master/README.md"
$DTE.ItemOperations.Navigate($readmeFile, [EnvDTE.vsNavigateOptions]::vsNavigateOptionsNewWindow)