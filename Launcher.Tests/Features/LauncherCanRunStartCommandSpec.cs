﻿using System.Diagnostics;
using System.IO;
using NSpec;
using System;

namespace Launcher.Tests.Features
{
    internal class LauncherCanRunStartCommandSpec : nspec
    {
        private Process StartLauncher(params string[] args)
        {
            var startInfo = new ProcessStartInfo
            {
                Arguments = ArgumentEscaper.Escape(args),
                FileName = "Launcher.exe",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var proc = Process.Start(startInfo);
            proc.should_not_be_null();
            proc.WaitForExit();
            return proc;
        }

        private void describe_()
        {
            before = () =>
            {
                var workingDirectory = Path.GetFullPath(Path.Combine(System.Reflection.Assembly.GetExecutingAssembly().CodeBase, "..").Replace("file:///", ""));
                Directory.SetCurrentDirectory(workingDirectory);
            };

            after = () => File.Delete(@"Fixtures\Bean.txt");

            describe["when started with insufficient arguments"] = () =>
            {

                it["outputs a message onto STDERR"] = () =>
                {
                    var launcher = StartLauncher();
                    launcher.StandardError.ReadToEnd().should_contain("Launcher was run with insufficient arguments");
                };

                it["returns an exit code of 1"] = () =>
                {
                    var launcher = StartLauncher();
                    launcher.ExitCode.should_be(1);
                };
            };


            describe["when a start command is provided to the Launcher"] = () =>
            {
                it["runs it"] = () =>
                {
                    StartLauncher("Fixtures", "CivetCat.bat bean1 bean2");

                    var beans = File.ReadAllText(@"Fixtures\Bean.txt").Split('\n');
                    beans[0].should_contain("bean1");
                    beans[1].should_contain("bean2");
                };

                it["returns the exit code from it"] = () =>
                {
                    var launcher = StartLauncher("Fixtures", @"CivetCat.bat");
                    launcher.ExitCode.should_be(0);

                    launcher = StartLauncher("Fixtures", "Exit.bat 5678");
                    launcher.ExitCode.should_be(5678);
                };

                it["propagates stdout from it"] = () =>
                {
                    var launcher = StartLauncher("Fixtures", "CivetCat.bat");
                    var stdout = launcher.StandardOutput.ReadToEnd();
                    stdout.should_contain("This is STDOUT");
                };

                it["propagates stderr from it"] = () =>
                {
                    var launcher = StartLauncher("Fixtures", "CivetCat.bat");
                    var stderr = launcher.StandardError.ReadToEnd();
                    stderr.should_contain("This is STDERR");
                };

                it["properly splits up arguments"] = () =>
                {
                    StartLauncher("Fixtures", "CivetCat.bat bean1 bean2");

                    var beans = File.ReadAllText(@"Fixtures\Bean.txt").Split('\n');
                    beans[0].should_contain("bean1");
                    beans[1].should_contain("bean2");

                    StartLauncher("Fixtures", "CivetCat.bat \"bean1 bean2\"");

                    var beans2 = File.ReadAllText(@"Fixtures\Bean.txt").Split('\n');
                    beans2[0].should_contain("bean1 bean2");
                };

                it["works with absolute paths with quoted arguments"] = () =>
                {
                    StartLauncher("Fixtures", "/Fixtures/CivetCat.bat \"bean1\" \"bean2\"");

                    var beans = File.ReadAllText(@"Fixtures\Bean.txt").Split('\n');
                    beans[0].should_contain("bean1");
                    beans[1].should_contain("bean2");
                };
            };
        }
    }
}