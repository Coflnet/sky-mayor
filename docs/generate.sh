# generates the c# client
VERSION=0.4.1
NAME=Coflnet.Sky.Mayor.Client

docker run --rm -v "${PWD}:/local" --network host -u $(id -u ${USER}):$(id -g ${USER})  openapitools/openapi-generator-cli generate \
-i http://0.0.0.0:8080/api/openapi/v1/openapi.json \
-g csharp \
-o /local/out --additional-properties=packageName=$NAME,packageVersion=$VERSION,licenseId=MIT,targetFramework=net8.0,library=restsharp


cd out
path=src/$NAME/$NAME.csproj
sed -i 's/GIT_USER_ID/Coflnet/g' $path
sed -i 's/GIT_REPO_ID/sky-mayor/g' $path
sed -i 's/>OpenAPI/>Coflnet/g' $path
sed -i 's@annotations</Nullable>@annotations</Nullable>\n    <PackageReadmeFile>README.md</PackageReadmeFile>@g' $path
sed -i '34i    <None Include="../../../../README.md" Pack="true" PackagePath="\"/>' $path

dotnet pack
cp src/$NAME/bin/Release/$NAME.*.nupkg ..
dotnet nuget push ../$NAME.$VERSION.nupkg --api-key $NUGET_API_KEY --source "nuget.org" --skip-duplicate
rm *.sln
