Find-Package Microsoft.AspNetCore.OpenApi -AllVersions
dotnet list .\src\ECommerce.API\ECommerce.API.csproj package --include-transitive
dotnet list .\src\ECommerce.API\ECommerce.API.csproj package --include-transitive
Get-Content "C:\Users\csban\OneDrive\Desktop\LearningC#\ECommerce\src\ECommerce.API\ECommerce.API.csproj"
->> NEW ONE BELOW 
M> Get-Project ECommerce.API

ProjectName                              Type                 FullName                                                                                       
-----------                              ----                 --------                                                                                       
ECommerce.API                            Unknown              C:\Users\csban\OneDrive\Desktop\LearningC#\ECommerce\src\ECommerce.API\ECommerce.API.csproj    


PM> Get-Project ECommerce.API

ProjectName                              Type                 FullName                                                                                       
-----------                              ----                 --------                                                                                       
ECommerce.API                            Unknown              C:\Users\csban\OneDrive\Desktop\LearningC#\ECommerce\src\ECommerce.API\ECommerce.API.csproj    

-- > Update-Package Microsoft.AspNetCore.OpenApi -Version 10.0.12


MIGRATION ->>
Add-Migration InitialCreate -Project ECommerce.Infrastructure -StartupProject ECommerce.API

