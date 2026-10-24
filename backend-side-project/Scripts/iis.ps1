# import module
Import-Module WebAdministration
cd IIS:\AppPools\
 

# gamePlay.API
cd D:\website\
new-item -Name api.gameplay -ItemType directory
New-Item IIS:\Sites\api.gameplay  -bindings @{protocol="http";bindingInformation=":3009:localhost"} -physicalPath D:\website\api.gameplay
New-item IIS:\AppPools\api.gameplay | Set-ItemProperty -Name "managedRuntimeVersion" -Value "v4.0"


# gamePlay.API (preDeploy)
cd D:\website
New-item -Name _preDeploy_admin_api.gameplay -ItemType directory
New-Item IIS:\Sites\_preDeploy_admin_api.gameplay -bindings @{protocol="http";bindingInformation=":15099:"} -physicalPath D:\website\_preDeploy_admin_api.gameplay
New-item IIS:\AppPools\_preDeploy_admin_api.gameplay | Set-ItemProperty -Name "managedRuntimeVersion" -Value "v4.0"

