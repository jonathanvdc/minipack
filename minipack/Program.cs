using System;
using System.Collections.Generic;
using Flame.Compiler;
using Flame.Front;
using Flame.Front.Cli;
using Flame.Front.Options;
using Pixie;

namespace Minipack
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            var log = new ConsoleLog(
                ConsoleEnvironment.AcquireConsole(),
                EmptyCompilerOptions.Instance);

            if (args.Length == 0)
            {
                ReportNoInput(log);
                return 1;
            }

            // Find out which sub-program we should use.
            Subprogram subprog;
            if (!subprograms.TryGetValue(args[0], out subprog))
            {
                ReportUnknownSubprogram(args[0], log);
                return 1;
            }

            // Create a filtered log to count errors.
            var filtLog = new FilteredLog(
                new FlagLogFilter(
                    true,
                    true,
                    true,
                    true,
                    FlagLogFilter.DefaultReclassificationRules),
                log);

            // Run the sub-program.
            subprog.Run(new ArraySegment<string>(args, 1, args.Length - 1), log);

            return filtLog.ErrorCount == 0 ? 0 : 1;
        }

        private static readonly Dictionary<string, Subprogram> subprograms =
            new Dictionary<string, Subprogram>()
        {
            { "deb-tree", DebTreeSubprogram.Instance }
        };

        private static void ReportNoInput(ConsoleLog log)
        {
            ConsoleExtensions.Write(log.Console, "minipack: ", log.ContrastForegroundColor);
            log.WriteEntry("nothing to do", log.WarningStyle, "no sub-program was specified");
            DescribeSubprograms(log);
        }

        private static void ReportUnknownSubprogram(string name, ConsoleLog log)
        {
            log.LogError(new LogEntry("unknown subprogram", string.Format("'{0}' is not a known sub-program.", name)));
            DescribeSubprograms(log);
        }

        private static void DescribeSubprograms(ConsoleLog log)
        {
            var listItems = new List<MarkupNode>();
            foreach (var pair in subprograms)
            {
                listItems.Add(
                    new MarkupNode(
                        NodeConstants.ListItemNodeType,
                        new MarkupNode[]
                        {
                            new MarkupNode(
                                NodeConstants.BrightNodeType,
                                pair.Key),

                            new MarkupNode(
                                NodeConstants.TextNodeType,
                                " -- " + pair.Value.Description),

                            new MarkupNode(
                                NodeConstants.TextNodeType,
                                Environment.NewLine + "    usage: minipack " + pair.Key + " " + pair.Value.Usage),
                        }));
            }
            var subprogList = ListExtensions.Instance.CreateList(listItems);
            log.LogMessage(new LogEntry("minipack subprograms", subprogList));
        }
    }
}
