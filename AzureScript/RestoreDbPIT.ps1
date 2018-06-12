﻿<#

# this script restores sql db to PIT(point in time)

# -- run only in first use --#
#Get-Module -ListAvailable AzureRM

#>


Param(
  #-- 3 firsts parameter using for get DB to restore 
  [Parameter(Mandatory=$True,HelpMessage="ResourceGroup of db to restore",Position=1) ]
  [string]$ResourceGroup,

  [Parameter(Mandatory=$True,HelpMessage="Server name of DB to restore",Position=2)]
  [string]$ServerName,

  [Parameter(Mandatory=$True,HelpMessage="Db name to restore",Position=3)]
  [string]$DbName,

  [Parameter(Mandatory=$true, HelpMessage="Date to restore, format:i.e dd/mm/yyyy",position=4)]
  [DateTime]$DateToRestore,

  [parameter(Mandatory=$true, HelpMessage= "Time to restore, format:i.e for 13:00:00 insert 13 ", position=5)]
  [string]$HourseToRestore,

  [Parameter(Mandatory=$true, HelpMessage="specify minutes to restore format:i.e for 13:25:00 insert 25", position=6)] 
  [string]$MinutesToRestore,

  [Parameter(Mandatory=$true, HelpMessage="Resource group to create DB", position=7)]
  [string]$ResourceToRestore,

  [Parameter(Mandatory=$true, HelpMessage="Name for new DB", position=8)]
  [string]$NewDbName


)


    try
        {
        #add hourse to date
        $DateToRestore = $DateToRestore.AddHours($HourseToRestore);

        #add minutes to Date
        $DateToRestore = $DateToRestore.AddMinutes($MinutesToRestore);
        
        #login to Azure
        Login-AzureRmAccount

        #Get Db To Restore
        $DatabaseToRestore = Get-AzureRmSqlDatabase -ResourceGroupName $ResourceGroup -ServerName $ServerName -DatabaseName $DbName


        Restore-AzureRmSqlDatabase -FromPointInTimeBackup -PointInTime $DateToRestore  -ResourceGroupName $DatabaseToRestore.ResourceGroupName -ServerName $DatabaseToRestore.ServerName -TargetDatabaseName $NewDbName -ResourceId $DatabaseToRestore.ResourceID -Edition "Standard" -ServiceObjectiveName "S0"

        }

  catch
       {
        $MessageError = $_.Exception.Message
        Write-Host $MessageError -ForegroundColor red -NoNewline

       }
                                                   