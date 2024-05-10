using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Text;

namespace UnityEditor.Scripting.Python.Tests
{
    class PackageIntegrityCheck
    {
        const string k_pythonMajorVersion = "3";
        const string k_pythonMinorVersion = "10";
        const string k_pythonVersion = k_pythonMajorVersion + "." + k_pythonMinorVersion;

        string installFolder = Path.GetFullPath("Library") + "/PythonInstall";

        /// <summary>
        /// Check that the platform matches.
        ///
        /// We test on all platforms. On the platform that we apply to,
        /// we should pass the tests. On the other platforms, we ignore
        /// the tests... except we need one to pass so we have one that
        /// tests nothing.
        /// </summary>
        static bool PlatformMatches()
        {
            var assyname = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return assyname.StartsWith("com.unity.scripting.python.windows");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return assyname.StartsWith("com.unity.scripting.python.macos");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return assyname.StartsWith("com.unity.scripting.python.linux");
            }
            return false;
        }

        static void WriteAndAppend(StringBuilder output, string msg)
        {
            Console.WriteLine(msg);
            if (output != null)
            {
                output.Append(msg);
            }
        }

        // Run a process with args and combine stdout/stderr into the output.
        // Return the exit code.
        static int Run(string process, string args, StringBuilder output = null)
        {
            using(var proc = new System.Diagnostics.Process())
            {
                proc.StartInfo.FileName = process;
                proc.StartInfo.Arguments = args;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.WorkingDirectory = System.IO.Directory.GetCurrentDirectory();
                proc.StartInfo.RedirectStandardOutput = true;
                proc.OutputDataReceived += (s, e) => WriteAndAppend(output, e.Data);
                proc.StartInfo.RedirectStandardError = true;
                proc.ErrorDataReceived += (s, e) => WriteAndAppend(output, e.Data);

                Console.WriteLine($"Running {proc.StartInfo.FileName} {proc.StartInfo.Arguments}");
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();
                return proc.ExitCode;
            }
        }

        /// <summary>
        /// Hack to get the current file's directory
        /// </summary>
        /// <param name="fileName">Leave it blank to the current file's directory</param>
        /// <returns></returns>
        private static string __DIR__([System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
        {
            return Path.GetDirectoryName(fileName);
        }

        [Test]
        public void TestPlatformMismatch ()
        {
            if (PlatformMatches()) {
                Assert.Ignore("The platform matches.");
            }
            // The platform doesn't match, so this test passes and the
            // other tests are ignored. We need at least one test to pass or
            // else we fail the testing!
        }

        [Test, Order(1)]
        public void UnpackingTest()
        {
            if (!PlatformMatches()) {
                Assert.Ignore("Not relevant on this platform");
            }

            var archiveLocation = Path.GetFullPath($"{__DIR__()}/../bin~/pybin.7z");
            Assert.That(File.Exists(archiveLocation));
            // If `installFolder` already exists, it means Python is already installed. In this case, we don't run the test
            // (this test is normally only run on CI)
            Assume.That(!Directory.Exists(installFolder));

            string kZip;
            string kSite;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                kZip = "Data/Tools/7z.exe";
                kSite = "Lib/site.py";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                kZip = "Unity.app/Contents/Tools/7za";
                kSite = $"lib/python{k_pythonVersion}/site.py";
            }
            else /*if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))*/
            {
                kZip = "Data/Tools/7za";
                kSite = $"lib/python{k_pythonVersion}/site.py";
            }

            string exec = $"{Path.GetDirectoryName(EditorApplication.applicationPath)}/{kZip}";
            string args = $"x \"{archiveLocation}\" -o\"{installFolder}\"";
            int exitcode = Run(exec, args);
            Assert.That(exitcode, Is.EqualTo(0));

            var expectedPath = $"Library/PythonInstall/{kSite}";
            Console.Write($"Checking for {expectedPath}\n");
            Assert.That(expectedPath, Does.Exist);
        }

        [Test, Order(2)]
        public void FileModificationsTests ()
        {
            if (!PlatformMatches()) {
                Assert.Ignore("Not relevant on this platform");
            }

            List<string> addedFiles = null ;
            List<string> removedFiles = null ;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                addedFiles = new List<string>
                {
                    "Scripts/pip.bat",
                    $"Scripts/pip{k_pythonMajorVersion}.bat",
                    $"Scripts/pip{k_pythonVersion}.bat",
                    "Scripts/easy_install.bat",
                    $"Scripts/easy_install-{k_pythonVersion}.bat"
                };
                removedFiles = new List<string>
                {
                    "Scripts/pip.exe",
                    "Scripts/pip3.exe",
                    $"Scripts/pip{k_pythonVersion}.exe",
                    "Scripts/easy_install.exe",
                    $"Scripts/easy_install-{k_pythonVersion}.exe"
                };
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){
                // Linux has technically more removed files than OSX, but they have the same added files.
                addedFiles = new List<string>
                {
                    "bin/pip",
                    $"bin/pip{k_pythonMajorVersion}",
                    $"bin/pip{k_pythonVersion}",
                };
                removedFiles = new List<string>
                {
                    "bin/2to3",
                    $"bin/2to3-{k_pythonVersion}",
                    $"bin/python{k_pythonMajorVersion}-config",
                    $"bin/python{k_pythonVersion}-config",
                    "bin/idle",
                    "bin/idle3"
                };
            }

            else{
                Assert.Fail("No file modification verification set on this platform");
            }

            foreach(var added in addedFiles)
            {
                FileAssert.Exists(Path.Combine(installFolder, added));
            }

            foreach (var removed in removedFiles)
            {
                FileAssert.DoesNotExist(Path.Combine(installFolder, removed));
            }
        }

        [Test, Order(3)]
        public void PackageModificationsTest ()
        {
            if (!PlatformMatches()) {
                Assert.Ignore("Not relevant on this platform");
            }

            var proc = new System.Diagnostics.Process();

            string pipPath;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pipPath = Path.Combine(installFolder, "Scripts", "pip.bat");
            }
            else
            {
                pipPath = Path.Combine(installFolder, "bin", "pip");
            }
            string exec = pipPath;
            string args = "--version";
            var output = new StringBuilder();
            int exitcode = Run(exec, args, output);
            Assert.That(exitcode, Is.EqualTo(0));
            // The 'from' part is important; pip does not releases `21.x.0`,
            // it releases `21.x`. We don't want to mis-identify 21.x with 21.x.y
            Assert.That(output.ToString(), Does.StartWith("pip 21.2.4 from"), 
                "If this test fails, update the version here and add it to the changelog.");
        }

        [Test, Order(4)]
        public void TestPythonVersion ()
        {
            if (!PlatformMatches()) {
                Assert.Ignore("Not relevant on this platform");
            }

            string exec;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                exec = Path.Combine(installFolder, "python");
            }
            else
            {
                exec = Path.Combine(installFolder, "bin", $"python{k_pythonMajorVersion}");
            }
            string args = "--version";
            var output = new StringBuilder();
            int exitcode = Run(exec, args, output);
            Assert.That(exitcode, Is.EqualTo(0));
            Assert.That(output.ToString(), Does.StartWith($"Python {k_pythonVersion}."));
        }
    }
}
