var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

var buildDir = Directory("./src/Example/bin") + Directory(configuration);
var hugoPath = Directory("./tools") + File("hugo" + (IsRunningOnWindows() ? ".exe" : ""));
var hugoUrl = IsRunningOnWindows()
    ? "https://github.com/gohugoio/hugo/releases/download/v0.27.1/hugo_0.27.1_Windows-64bit.zip"
    : "https://github.com/gohugoio/hugo/releases/download/v0.27.1/hugo_0.27.1_Linux-64bit.tar.gz";

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
        DownloadFile(hugoUrl, tmpArchive);
        var tmpArchiveExtracted = Directory("./tools/hugo-zip");
        if(DirectoryExists(tmpArchiveExtracted))
        {
            DeleteDirectory(tmpArchiveExtracted, true);
        }
        CreateDirectory(tmpArchiveExtracted);
        if(IsRunningOnWindows())
        {
            Unzip(tmpArchive, tmpArchiveExtracted);
            CopyFile(tmpArchiveExtracted + File("hugo.exe"), hugoPath);
        }
        else
        {
            StartProcess("bash", new ProcessSettings() {
                Arguments = "-c \"tar xf " + tmpArchive + " -C " + tmpArchiveExtracted + "\""
            });
            CopyFile(tmpArchiveExtracted + File("hugo"), hugoPath);
        }
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

Task("New")
    .IsDependentOn("DownloadHugo")
    .Does(() =>
{
    Console.WriteLine("Enter the name for the new post...");
    var name = Console.ReadLine();
    if(string.IsNullOrEmpty(name))
    {
        Error("Name is empty...");
        return;
    }

    name = name.ToLower().Trim();

    foreach (char c in System.IO.Path.GetInvalidFileNameChars())
    {
        name = name.Replace(c, '_');
    }

    if(string.IsNullOrEmpty(name))
    {
        Error("Name is empty...");
        return;
    }

    StartProcess(hugoPath, new ProcessSettings() {
        Arguments = "new post/" + name + ".md",
        WorkingDirectory = Directory("./web")
    });
});

Task("Default")
    .IsDependentOn("Build");

RunTarget(target);
