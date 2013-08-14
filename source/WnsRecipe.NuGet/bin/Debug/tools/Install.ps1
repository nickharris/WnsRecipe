param($installPath, $toolsPath, $package, $project)

$path = [System.IO.Path]
$readmeFile = $path::Combine($path::GetDirectoryName($project.FileName), "App_Readme\WnsRecipe.Readme.htm")
$DTE.ItemOperations.Navigate($readmeFile, [EnvDTE.vsNavigateOptions]::vsNavigateOptionsNewWindow)