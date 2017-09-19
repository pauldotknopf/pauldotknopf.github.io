var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var buildDir = Directory("./src/Example/bin") + Directory(configuration);
var hugoPath = Directory("./tools") + File("hugo.exe");

Task("Clean")
    .Does(() =>
{
    Information(hugoPath);
    CleanDirectory(buildDir);
});

Task("DownloadHugo")
    .Does(() =>
{
    if(!FileExists(hugoPath))
    {
        var tmpArchive = Directory("./tools") + File("hugo-zip.zip");
        if(FileExists(tmpArchive))
        {
            DeleteFile(tmpArchive);
        }
        DownloadFile("https://github.com/gohugoio/hugo/releases/download/v0.27.1/hugo_0.27.1_Windows-64bit.zip", tmpArchive);
        var tmpArchiveExtracted = Directory("./tools/hugo-zip");
        if(DirectoryExists(tmpArchiveExtracted))
        {
            DeleteDirectory(tmpArchiveExtracted, true);
        }
        CreateDirectory(tmpArchiveExtracted);
        Unzip(tmpArchive, tmpArchiveExtracted);
        CopyFile(tmpArchiveExtracted + File("hugo.exe"), hugoPath);
        DeleteDirectory(tmpArchiveExtracted, true);
        DeleteFile(tmpArchive);
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("DownloadHugo")
    .Does(() =>
{
    StartProcess(hugoPath, new ProcessSettings() {
        WorkingDirectory = Directory("./web")
    });
});

Task("Server")
    .IsDependentOn("DownloadHugo")
    .Does(() =>
{
    StartProcess(hugoPath, new ProcessSettings() {
        Arguments = "server -D",
        WorkingDirectory = Directory("./web")
    });
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);
