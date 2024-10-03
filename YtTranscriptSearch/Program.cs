using System.Diagnostics;
using YoutubeExplode;
using YtTranscriptSearch.Shared;

string? channelNameOrUrl = GatherUserInput("Enter channel name or channel url (e.g., @MyChannelName):");

if (channelNameOrUrl == null)
{
    Exit();
    return;
}

string? searchTerms = GatherUserInput("Enter search terms:");

if (searchTerms == null)
{
    Exit();
    return;
}

List<TranscriptSearchMatch> allMatches = [];
CancellationTokenSource cancellationTokenSource = new();
CancellationToken cancellationToken = cancellationTokenSource.Token;
YoutubeClient youtubeClient = new();

bool showConsoleMessage = true;

TranscriptSearchParameters transcriptSearchParams = await TranscriptSearchHelpers.CreateAsync(youtubeClient, channelNameOrUrl, searchTerms);

transcriptSearchParams.Matches = allMatches;
transcriptSearchParams.Progress = new Progress<TranscriptSearchProgress>((prog) =>
{
    if (prog.Exception != null)
    {
        ConsolePrint($"[{DateTime.Now:g}] Failed to grab transcript for '{prog.VideoTitle}' ({prog.Exception.Message})", ConsoleColor.DarkRed);
        return;
    }

    bool foundAnyMatches = prog.Matches?.Length > 0;
    ConsolePrint($"[{DateTime.Now:g}] {prog.VideoTitle} - {prog.Matches?.Length ?? 0} matches", foundAnyMatches ? ConsoleColor.DarkGreen : ConsoleColor.Gray);
});

TranscriptSearch transcriptSearch = new();

// begin main task, in the background
Task mainTask = transcriptSearch.SearchAsync(transcriptSearchParams, cancellationToken);

// wait for user input while main task is running
do
{
    Console.ReadLine();

    if (!ShowYesNoPrompt("Are you sure you want to cancel the search?"))
    {
        continue;
    }

    await cancellationTokenSource.CancelAsync();

    ShowSaveProgressPrompt();

} while (!cancellationToken.IsCancellationRequested);

// main functions
void ShowFolderOpenPrompt(string file)
{
    string? path = Path.GetDirectoryName(Path.GetFullPath(file));

    if (path == null)
    {
        return;
    }

    if (!ShowYesNoPrompt($"Would you like to open '{path}'?", false))
    {
        Process.Start("explorer.exe", path);
    }
}

void ShowSaveProgressPrompt()
{
    if (ShowYesNoPrompt("Would you like to save all progress?"))
    {
        string file = $"{transcriptSearchParams.Channel.Title} - {searchTerms}.csv";
        allMatches.Save(file);
        ShowFolderOpenPrompt(file);
    }
}

bool ShowYesNoPrompt(string promptMsg, bool defaultValue = true)
{
    showConsoleMessage = false;
    char yesNoDefaultValue = defaultValue ? 'y' : 'n';
    Console.WriteLine($"{promptMsg} [{yesNoDefaultValue}]:");
    string? input = Console.ReadLine();
    showConsoleMessage = true;
    return string.IsNullOrWhiteSpace(input) || input.StartsWith(yesNoDefaultValue) == true;
}

void ConsolePrint(string text, ConsoleColor consoleColor)
{
    if (!showConsoleMessage)
    {
        return;
    }

    Console.ForegroundColor = consoleColor;
    Console.WriteLine(text);
    Console.ResetColor();
}

static string? GatherUserInput(string promptMsg)
{
    Console.WriteLine(promptMsg);
    string? myValue = Console.ReadLine();
    return string.IsNullOrWhiteSpace(myValue) ? null : myValue;
}

static void Exit()
{
    Console.WriteLine("Exiting...");
    Environment.Exit(Environment.ExitCode);
}
