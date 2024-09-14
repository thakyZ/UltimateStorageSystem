using namespace System;
using namespace System.Collections;
using namespace System.IO;
using namespace System.Management.Automation;
using namespace System.Text.RegularExpressions;

[CmdletBinding(DefaultParameterSetName = 'SimpleCompile')]
Param(
  # Specifies the build configuration to use.
  [Parameter(Mandatory = $False,
             HelpMessage = 'The build configuration to use.')]
  [string]
  $Configuration = "Release",
  # Specifies a switch to run the task for before core compile.
  [Parameter(Mandatory = $True,
             ParameterSetName = 'BeforeCoreCompile',
             HelpMessage = 'Runs the task for before core compile.')]
  [Alias('Before')]
  [switch]
  $BeforeCoreCompile = $False,
  # Specifies a switch to run the task for after building the mod.
  [Parameter(Mandatory = $True,
             ParameterSetName = 'AfterBuild',
             HelpMessage = 'Runs the task for after building the mod.')]
  [Alias('After')]
  [switch]
  $AfterBuild = $False,
  # Specifies a switch to compile the entire mod.
  [Parameter(Mandatory = $False,
             ParameterSetName = 'SimpleCompile',
             HelpMessage = 'Compiles the entire mod.')]
  [Alias('Compile', 'Simple')]
  [switch]
  $SimpleCompile = $True
)

Begin {

  Function Exit-WithCode {
    [CmdletBinding()]
    Param(
      [Parameter(Mandatory = $False,
                 Position = 0)]
      [int]
      $Code = 1,
      [Parameter(Mandatory = $False)]
      [AllowNull()]
      [Exception]
      $Exception = $Null,
      [Parameter(Mandatory = $False)]
      [AllowNull()]
      [ErrorRecord]
      $ErrorRecord = $Null,
      [Parameter(Mandatory = $False)]
      [AllowNull()]
      [object]
      $Message = $Null
    )

    Try {
      If ($Null -ne (Get-Variable -Scope Global -ValueOnly -Name 'OriginalDebugPreference')) {
        $DebugPreference   = (Get-Variable -Scope Global -ValueOnly -Name 'OriginalDebugPreference');
      }

      If ($Null -ne (Get-Variable -Scope Global -ValueOnly -Name 'OriginalVerbosePreference')) {
        $VerbosePreference = (Get-Variable -Scope Global -ValueOnly -Name 'OriginalVerbosePreference');
      }

      If ($Null -ne (Get-Variable -Scope Global -ValueOnly -Name 'OriginalErrorActionPreference')) {
        $ErrorActionPreference   = (Get-Variable -Scope Global -ValueOnly -Name 'OriginalErrorActionPreference');
      }

      Remove-Variable -Scope Global -ErrorAction Continue -Name 'OriginalDebugPreference';
      Remove-Variable -Scope Global -ErrorAction Continue -Name 'OriginalVerbosePreference';
      Remove-Variable -Scope Global -ErrorAction Continue -Name 'OriginalErrorActionPreference';

      If ($Null -ne $Exception) {
        If ($Null -ne $Message) {
          Write-Error -Exception $Exception -Message $Message | Out-Host;
        } Else {
          Write-Error -Exception $Exception | Out-Host;
        }
      } ElseIf ($Null -ne $ErrorRecord) {
        If ($Null -ne $Message) {
          Write-Host -ForegroundColor Red -Object $Message | Out-Host;
        }

        Write-Error -ErrorRecord $ErrorRecord | Out-Host;
      } Else {
        If ($Null -ne $Message) {
          Write-Host -Object $Message | Out-Host;
        }
      }
    } Catch {
      Write-Host -ForegroundColor Red -Object "Failed at Main block in Exit-WithCode method";
      Write-Error -ErrorRecord $ErrorRecord | Out-Host;
    }

    Exit $Code;
  }

  Try {
    $global:OriginalDebugPreference = $DebugPreference;
    If ($PSBoundParameters.ContainsKey('Debug')) {
      $DebugPreference = 'Continue';
    }
    $global:OriginalVerbosePreference = $VerbosePreference;
    If ($PSBoundParameters.ContainsKey('Verbose')) {
      $VerbosePreference = 'Continue';
    }
    $global:OriginalErrorActionPreference = $ErrorActionPreference;
    $ErrorActionPreference = 'Stop';

    Function Get-SolutionLocation {
      [CmdletBinding()]
      [OutputType([DirectoryInfo])]
      Param([DirectoryInfo] $PSScriptRootParent, [string] $ProjectName)

      Begin {
        Try {
          [DirectoryInfo] $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Begin block in Get-SolutionLocation method";
        }
      } Process {
        Try {
          [FileSystemInfo[]] $ProjectFiles         = (Get-ChildItem -LiteralPath $PSScriptRootParent -Recurse -File);
          [FileSystemInfo[]] $ProjectFilesFiltered = ($ProjectFiles | Where-Object { $_.BaseName -eq $ProjectName -and $_.Extension -eq '.sln' });
          $Output = ($ProjectFilesFiltered | Select-Object -First 1).Directory;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in Get-SolutionLocation method";
        }
      } End {
        Try {
          If ($Null -eq $Output) {
            Throw '$Output is not DirectoryInfo it is instead $Null';
          }
          If ($Output -isnot [DirectoryInfo]) {
            Throw "`$Output is not DirectoryInfo it is instead $($Output.GetType().FullName)"
          }
          Write-Output -NoEnumerate -InputObject $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at End block in Get-SolutionLocation method";
        }
      }
    }

    Function Get-CsProjectLocation {
      [CmdletBinding()]
      [OutputType([DirectoryInfo])]
      Param([DirectoryInfo] $SolutionLocation, [string] $ProjectName)

      Begin {
        Try {
          [DirectoryInfo] $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Begin block in Get-CsProjectLocation method";
        }
      } Process {
        Try {
          [FileSystemInfo[]] $ProjectFiles         = (Get-ChildItem -LiteralPath $SolutionLocation -Recurse -File);
          [FileSystemInfo[]] $ProjectFilesFiltered = ($ProjectFiles | Where-Object { $_.BaseName -eq $ProjectName -and $_.Extension -eq '.csproj' });
          $Output = ($ProjectFilesFiltered | Select-Object -First 1).Directory;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in Get-CsProjectLocation method";
        }
      } End {
        Try {
          If ($Null -eq $Output) {
            Throw '$Output is not DirectoryInfo it is instead $Null';
          }
          If ($Output -isnot [DirectoryInfo]) {
            Throw "`$Output is not DirectoryInfo it is instead $($Output.GetType().FullName)"
          }
          Write-Output -NoEnumerate -InputObject $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at End block in Get-CsProjectLocation method";
        }
      }
    }

    Function Get-ModManifest {
      [CmdletBinding()]
      [OutputType([FileInfo])]
      Param([DirectoryInfo] $CsProjectLocation)

      Begin {
        Try {
          [FileInfo] $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Begin block in Get-ModManifest method";
        }
      } Process {
        Try {
          [FileSystemInfo[]] $ProjectFiles         = (Get-ChildItem -LiteralPath $CsProjectLocation -Recurse -File);
          [FileSystemInfo[]] $ProjectFilesFiltered = ($ProjectFiles | Where-Object { $_.BaseName -eq 'manifest' -and $_.Extension -eq '.json' });
          $Output = ($ProjectFilesFiltered | Select-Object -First 1);
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in Get-ModManifest method";
        }
      } End {
        Try {
          If ($Null -eq $Output) {
            Throw '$Output is not FileInfo it is instead $Null';
          }
          If ($Output -isnot [FileInfo]) {
            Throw "`$Output is not FileInfo it is instead $($Output.GetType().FullName)"
          }
          Write-Output -NoEnumerate -InputObject $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at End block in Get-ModManifest method";
        }
      }
    }

    Function ConvertTo-Hashtable {
      [CmdletBinding()]
      Param(
        [Parameter(Mandatory = $True,
                  Position = 0,
                  ValueFromPipeline = $True,
                  HelpMessage = 'Object to convert from PSObject to Hashtable.')]
        [ValidateNotNull()]
        [PSCustomObject]
        $InputObject
      )

      Process {
        Try {
          If ($Null -eq $InputObject) {
            Return $Null;
          }

          If ($InputObject -is [IEnumerable] -and $InputObject -isnot [string]) {
              $Collection = @(ForEach ($Object in $InputObject) {
                              ConvertTo-Hashtable -InputObject $Object;
                            })
              Write-Output -NoEnumerate -InputObject $Collection
          } ElseIf ($InputObject -is [PSObject]) {
            [Hashtable] $Hash = @{}

              ForEach ($Property in $InputObject.PSObject.Properties) {
                  $Hash[$Property.Name] = (ConvertTo-Hashtable -InputObject $Property.Value).PSObject.BaseObject
              }

              Write-Output -NoEnumerate -InputObject $Hash;
          } Else {
            Write-Output -NoEnumerate -InputObject $InputObject;
          }
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in ConvertTo-Hashtable method";
        }
      }
    }

    Function Invoke-Pause {
      [CmdletBinding()]
      Param()

      Process {
        Try {
          Write-Host -Object 'Press any key to continue...' -NoNewLine | Out-Host;
          Read-Host;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in Invoke-Pause method";
        }
      }
    }

    Function Invoke-QueryHashtable {
      [CmdletBinding()]
      Param(
        [Parameter(Mandatory = $True,
                  Position = 0,
                  ValueFromPipeline = $True,
                  HelpMessage = 'Object to convert from PSObject to Hashtable.')]
        [Alias('Hashtable')]
        [ValidateNotNullOrEmpty()]
        [object]
        $InputObject,
        [Parameter(Mandatory = $True,
                  Position = 0,
                  ValueFromPipeline = $True,
                  HelpMessage = 'Dot notation string to query value.')]
        [ValidatePattern('[\w\d]+(\.[\w\d]+)*')]
        [ValidateNotNullOrEmpty()]
        [string]
        $Query
      )

      Begin {
        Try {
          If ($Null -eq $InputObject) {
            Write-Output -InputObject '$InputObject' $InputObject | Out-Host;
            Return $Null;
          }

          Write-Output -InputObject '$Query' $Query | Out-Host;
          [string[]] $QuerySplit = ($Query -split '\.', 2);
          Write-Output -InputObject '$QuerySplit[0]' $QuerySplit[0] | Out-Host;
          [object]   $Output;
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Begin block in Invoke-QueryHashtable method";
        }
      } Process {
        Try {
          [int] $QueryIndex = -1;
          If ($InputObject -is [IEnumerable] -and $InputObject -isnot [string] -and [int]::TryParse($QuerySplit[0], [ref] $QueryIndex)) {
            Write-Output -InputObject '$QueryIndex' $QueryIndex | Out-Host;
            For ($Index = 0; $Index -lt $InputObject.Count(); $Index++) {
              If ($Index -eq $QueryIndex) {
                Write-Output -InputObject '$Index' $Index | Out-Host;
                Write-Output -InputObject '$InputObject' $InputObject | Out-Host;
                Write-Output -InputObject (Invoke-QueryHashtable -InputObject $InputObject[$Index] -Query $QuerySplit[1]);
              }
            }
          } ElseIf ($InputObject -is [Hashtable]) {
            ForEach ($Key in $InputObject.Keys) {
              If ($Key -eq $QuerySplit[0]) {
                Write-Output -InputObject '$Key' $Key | Out-Host;
                $Value = $InputObject.Item($Key);
                Write-Output -InputObject '$InputObject' $InputObject | Out-Host;
                Write-Output -NoEnumerate -InputObject (Invoke-QueryHashtable -InputObject $Value -Query $QuerySplit[1])
              }
            }
          } Else {
            Write-Output -InputObject @('$InputObject', $InputObject) | Out-Host;
            Write-Output -NoEnumerate -InputObject $InputObject;
          }
        } Catch {
          Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in Invoke-QueryHashtable method";
        }
      }
    }

    [DirectoryInfo]    $PSScriptRootDirectory      = (Get-Item -LiteralPath $PSScriptRoot);
    [string]           $PSScriptRootParentPath     = (Join-Path -Path $PSScriptRootDirectory -ChildPath "..");
    [PathInfo]         $PSScriptRootParentResolved = (Resolve-Path -LiteralPath $PSScriptRootParentPath);
    [DirectoryInfo]    $PSScriptRootParent         = (Get-Item -LiteralPath $PSScriptRootParentResolved);
    [FileSystemInfo[]] $ProjectFiles               = (Get-ChildItem -LiteralPath $PSScriptRootParent.FullName);
    [string]           $ProjectName                = $PSScriptRootParent.Name;
    [DirectoryInfo]    $SolutionLocation           = (Get-SolutionLocation  -ProjectName $ProjectName -PSScriptRootParent $PSScriptRootParent.FullName)[1];
    [DirectoryInfo]    $CsProjectLocation          = (Get-CsProjectLocation -ProjectName $ProjectName -SolutionLocation   $SolutionLocation.FullName)[1];
    [FileInfo]         $ModManifest                = (Get-ModManifest -CsProjectLocation $CsProjectLocation)[1];
    [HashTable]        $ModManifestJson            = (Get-Content -LiteralPath $ModManifest.FullName | ConvertFrom-Json | ConvertTo-HashTable);
    [FileInfo[]]       $CSharpFiles                = (Get-ChildItem -LiteralPath $CsProjectLocation.FullName -Recurse -File | Where-Object { $_.FullName -notmatch '[\\/](?:obj|bin)[\\/]' -and $_.Extension -eq '.cs' });
    [FileInfo[]]       $FilesToEdit                = @();
    [string]           $VariableReplaceRegex       = '\{\{([^\}]+)\}\}';
  } Catch {
    Exit-WithCode -ErrorRecord $_ -Message "Failed at Begin block in main method";
  }
} Process {
  Try {
    If ($BeforeCoreCompile) {
      [FileInfo[]] $CSharpFilesFiltered = @($CSharpFiles | Where-Object { $Null -ne (Get-Content -LiteralPath $_.FullName | Select-String -Pattern $VariableReplaceRegex) });
      # Make backup of files
      ForEach ($File in $CSharpFilesFiltered) {
        [string] $BackupFilePath = (Join-Path -Path $File.Directory.FullName -ChildPath "$($File.Name).bak");
        If (Test-Path -LiteralPath $BackupFilePath -PathType Leaf) {
          Write-Warning -Message "Backup file already exists at $($BackupFilePath) please check if it is actually a backup file.";
          Invoke-Pause;
          $FilesToEdit += $File;
          Continue;
        } ElseIf (Test-Path -LiteralPath $BackupFilePath -PathType Container) {
          Throw "Backup file at $($BackupFilePath) was invalid expected a Leaf got a container.";
        }
        Copy-Item -LiteralPath $File.Fullname -Destination $BackupFilePath -ErrorAction Stop;
        $FilesToEdit += $File;
      }

      ForEach ($File in $FilesToEdit) {
        [string[]] $Content    = (Get-Content -LiteralPath $File.FullName);
        [string[]] $OutContent = @();
        [Regex]    $Regex      = [Regex]::new($VariableReplaceRegex);

        ForEach ($Line in $Content) {
          [MatchCollection] $_Matches = $Regex.Matches($Line);

          ForEach ($Match in $_Matches) {
            [GroupCollection] $Groups = $Match.Groups;

            If ($Groups.Count -gt 1) {
              [Group] $GroupToReplace = $Groups[0];
              [Group] $GroupWithKey = $Groups[1];
              [string] $Key = $GroupWithKey.Value;
              Write-Output -InputObject "`$GroupWithKey.Value = $($GroupWithKey.Value)" | Out-Host;
              $Test = (Invoke-QueryHashtable -Hashtable $ModManifestJson -Query $Key);
              Write-Host -NoNewLine -Object '$Test [' | Out-Host;
              If ($Null -eq $Test) {
                Write-Host -NoNewLine -Object 'null' -ForegroundColor Blue | Out-Host;
              } Else {
                Write-Host -NoNewLine -Object $Test.GetType().FullName | Out-Host;
              }
              Write-Host -NoNewLine -Object ']: ' | Out-Host;
              Write-Output -InputObject $Test | Out-Host;
              [string] $Value = $Test[0];

              $OutContent += $Line.Replace($GroupToReplace.Value, $Value);
            }
          }
        }

        Set-Content -LiteralPath $File.Fullname -Value $OutContent;
      }
    } ElseIf ($AfterBuild) {
      # Restore backup of files
      ForEach ($File in @($CSharpFiles | Where-Object { $_.Name.EndsWith('.cs.bak') })) {
        [string] $FilePath = (Join-Path -Path $_.Directory.FullName -ChildPath $_.BaseName);
        If (Test-Path -LiteralPath $FilePath -PathType Leaf) {
          Remove-Item -LiteralPath $FilePath -ErrorAction Stop;
        } ElseIf (Test-Path -LiteralPath $FilePath -PathType Container) {
          Throw "File at $($FilePath) was invalid expected a Leaf got a container.";
        }
        Move-Item -LiteralPath $_ -Destination $FilePath -ErrorAction Stop;
      }
    } ElseIf ($Compile) {
      $DotNetCommand = (Get-Command -Name "dotnet" -ErrorAction SilentlyContinue);

      If ($Null -eq $DotNetCommand) {
        Throw 'Command "dotnet" was not found on the system path.';
      }

      Start-Process -FilePath $DotNetCommand.Source -ArgumentList @('build', (Join-Path -Path $SolutionLocation -ChildPath "$($ProjectName).sln"), "-property:Configuration=$($Configuration)") -Wait;
    }
  } Catch {
    Exit-WithCode -ErrorRecord $_ -Message "Failed at Process block in main method";
  }
} End {
  Exit-WithCode -Code 0 -Message "Finished";
}