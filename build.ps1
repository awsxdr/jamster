rm -r ./output

cd ./amethyst.ui
npm i
npm run build
cd ..

cd ./amethyst
dotnet restore
cd ..

cd ./amethyst.tests
dotnet test
cd ..

cd ./amethyst
dotnet publish ./amethyst.csproj /p:PublishProfile=./Properties/PublishProfiles/WinX64.pubxml
dotnet publish ./amethyst.csproj /p:PublishProfile=./Properties/PublishProfiles/LinuxX64.pubxml
dotnet publish ./amethyst.csproj /p:PublishProfile=./Properties/PublishProfiles/LinuxArm64.pubxml
cd ..

mkdir ./output
mkdir ./output/bin
mkdir ./output/bin/win-x64
mkdir ./output/bin/linux-x64
mkdir ./output/bin/linux-arm64
mv ./amethyst/bin/Release/net8.0/win-x64/publish/** ./output/bin/win-x64/
mv ./amethyst/bin/Release/net8.0/linux-x64/publish/** ./output/bin/linux-x64/
mv ./amethyst/bin/Release/net8.0/linux-arm64/publish/** ./output/bin/linux-arm64/
cp -r ./output/bin/win-x64/wwwroot ./output/wwwroot
rm -r ./output/bin/win-x64/wwwroot
rm -r ./output/bin/linux-x64/wwwroot
rm -r ./output/bin/linux-arm64/wwwroot
cp ./start.cmd ./output/