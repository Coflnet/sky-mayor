# generates the c# client
VERSION=0.0.1
NAME=Coflnet.Sky.Mayor.Client

docker run --rm -v "${PWD}:/local" --network host -u $(id -u ${USER}):$(id -g ${USER})  openapitools/openapi-generator-cli generate \
-i /local/swagger.json \
-g csharp-netcore \
-o /local/out --additional-properties=packageName=$NAME,packageVersion=$VERSION,licenseId=MIT

cd out
sed -i 's/GIT_USER_ID/Coflnet/g' src/$NAME/$NAME.csproj
sed -i 's/GIT_REPO_ID/SkyApi/g' src/$NAME/$NAME.csproj
sed -i 's/>OpenAPI/>Coflnet/g' src/$NAME/$NAME.csproj

dotnet pack
cp src/$NAME/bin/Debug/$NAME.*.nupkg ..