param($installPath, $toolsPath, $package, $project)

$regSubkey = "\EnterpriseTools\QualityTools\TestTypes\{13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b}\TestTypeExtensions\AsyncTestClassAttribute"
$regName = "AttributeProvider"
$regValue = "Nito.AsyncEx.UnitTests.AsyncTestClassAttribute, Nito.AsyncEx.UnitTests.MSTest"

$machineReg = "hklm:\" + $dte.RegistryRoot
$userReg = "hkcu:\" + $dte.RegistryRoot
$machineConfigReg = "hklm:\" + $dte.RegistryRoot + "_Config"
$userConfigReg = "hkcu:\" + $dte.RegistryRoot + "_Config"

$installDir = (Get-ItemProperty -Path ($machineReg + "\Setup\VS")).ProductDir + "Common7\IDE\PrivateAssemblies"
$existingDllPath = $installDir + "\Nito.AsyncEx.UnitTests.MSTest.dll"
$myDllPath = $toolsPath + "\Nito.AsyncEx.UnitTests.MSTest.dll"
$overwriteDlls = $true

if (Test-Path $existingDllPath)
{
  $existingDll = Get-Command $existingDllPath
  $existingDllVersion = New-Object Version -ArgumentList $existingDll.FileVersionInfo.FileVersion
  $myDll = Get-Command $myDllPath
  $myDllVersion = New-Object Version -ArgumentList $myDll.FileVersionInfo.FileVersion
  $overwriteDlls = $myDllVersion -gt $existingDllVersion
}

function Register($regRoot)
{
  $regKey = $regRoot + $regSubKey
  if (Test-Path $regKey)
  {
    New-ItemProperty -Path $regKey -Name $regName -Value $regValue -Force
  }
  else
  {
    New-Item $regKey -Force
    New-ItemProperty -Path $regKey -Name $regName -Value $regValue
  }
}

if ($overwriteDlls)
{
  if (Test-Path $existingDllPath)
  {
    if (Test-Path ($existingDllPath + ".old"))
    {
      Remove-Item ($existingDllPath + ".old")
    }

    Rename-Item $existingDllPath ($existingDllPath + ".old")
  }

  Copy-Item -Path $myDllPath -Destination $existingDllPath
  Register $machineReg
  Register $userReg
  Register $machineConfigReg
  Register $userConfigReg
  [System.Windows.MessageBox]::Show("You must restart Visual Studio for the changes to take effect.", "Restart required", "OK", "Exclamation")
}
