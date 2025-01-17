using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Robust.Shared.ContentPack;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Utility;

namespace Robust.Shared.Console.Commands
{
    [UsedImplicitly]
    internal sealed class ExecCommand : IConsoleCommand
    {
        private static readonly Regex CommentRegex = new Regex(@"^\s*#");

        public string Command => "exec";
        public string Description => Loc.GetString("cmd-exec-desc");
        public string Help => Loc.GetString("cmd-exec-help");

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var res = IoCManager.Resolve<IResourceManager>();

            if (args.Length < 1)
            {
                shell.WriteError("No file specified!");
                return;
            }

            var path = new ResourcePath(args[0]).ToRootedPath();
            if (!res.UserData.Exists(path))
            {
                shell.WriteError("File does not exist.");
                return;
            }

            using var text = res.UserData.OpenText(path);
            while (true)
            {
                var line = text.ReadLine();
                if (line == null)
                {
                    return;
                }

                if (string.IsNullOrWhiteSpace(line) || CommentRegex.IsMatch(line))
                {
                    // Comment or whitespace.
                    continue;
                }

                shell.ConsoleHost.AppendCommand(line);
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var res = IoCManager.Resolve<IResourceManager>();

                var hint = Loc.GetString("cmd-exec-arg-filename");
                var options = CompletionHelper.UserFilePath(args[0], res.UserData);

                return CompletionResult.FromHintOptions(options, hint);
            }

            return CompletionResult.Empty;
        }
    }
}
