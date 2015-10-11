$PackageDirectory = "C:\Users\kelvi_000\Documents\Crm 2015\SDK\Tools\PackageDeployer"
cd $PackageDirectory
Add-PSSnapin Microsoft.Xrm.Tooling.Connector
Add-PSSnapin Microsoft.Xrm.Tooling.PackageDeployment
$CRMConn = Get-CrmConnection -DeploymentRegion Oceania -OnlineType Office365 -OrganizationName Augendev -Credential $Cred
Import-CrmPackage -CrmConnection $CRMConn -PackageDirectory $PackageDirectory -PackageName crmpackagedeployment.dll -Verbose