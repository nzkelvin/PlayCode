$PackageDirectory = $pwd
cd $PackageDirectory
Add-PSSnapin Microsoft.Xrm.Tooling.Connector
Add-PSSnapin Microsoft.Xrm.Tooling.PackageDeployment
$CRMConn = Get-CrmConnection -DeploymentRegion Oceania -OnlineType Office365 -OrganizationName mrxrm -Credential $Cred
Import-CrmPackage -CrmConnection $CRMConn -PackageDirectory $PackageDirectory -PackageName crmpackagedeployment.dll -Verbose