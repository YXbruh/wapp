using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace CSA.Services
{
    /// <summary>
    /// Controls a VirtualBox virtual machine by shelling out to the VBoxManage
    /// command-line tool. Used by the lab pages so a lecturer can preview a lab
    /// environment and a student can start one to work in.
    ///
    /// Every method returns a human-readable success or failure message; nothing
    /// throws out of this class, so a failed VM command can never take the page down.
    /// </summary>
    public static class VirtualMachineManager
    {
        /// <summary>How long to wait for a VBoxManage command before giving up.</summary>
        private const int CommandTimeoutMs = 60000;

        /// <summary>
        /// VM names are pasted straight into a command line, so only allow a safe
        /// character set. This stops a crafted name injecting extra arguments.
        /// </summary>
        private static readonly Regex SafeVmName = new Regex(@"^[A-Za-z0-9 _.\-]{1,64}$");

        /// <summary>
        /// Full path to VBoxManage.exe. Overridable in Web.config
        /// (&lt;appSettings&gt;&lt;add key="VBoxManagePath" value="..." /&gt;) for machines
        /// where VirtualBox is installed somewhere other than the default location.
        /// </summary>
        private static string VBoxManagePath
        {
            get
            {
                string configured = ConfigurationManager.AppSettings["VBoxManagePath"];
                if (!string.IsNullOrWhiteSpace(configured)) return configured;

                string defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    @"Oracle\VirtualBox\VBoxManage.exe");

                // Fall back to the bare name so it can still be found via PATH.
                return File.Exists(defaultPath) ? defaultPath : "VBoxManage.exe";
            }
        }

        // ==================================================================
        // Public operations
        // ==================================================================

        /// <summary>
        /// Powers on the VM with a visible desktop window.
        /// Runs: VBoxManage startvm "vmName" --type gui
        /// </summary>
        public static string StartVM(string vmName)
        {
            if (!IsNameValid(vmName)) return InvalidNameMessage(vmName);

            // Starting an already-running VM is an error in VBoxManage, so report the
            // situation plainly instead of surfacing its exit code.
            string state = GetVMState(vmName);
            if (state == "running")   return $"'{vmName}' is already running.";
            if (state == "paused")    return $"'{vmName}' is paused — use Resume instead.";

            VBoxResult result = Run($"startvm \"{vmName}\" --type gui");

            return result.Succeeded
                ? $"'{vmName}' started successfully."
                : $"Could not start '{vmName}'. {result.ErrorSummary}";
        }

        /// <summary>
        /// Powers the VM off immediately, the equivalent of pulling its plug.
        /// Runs: VBoxManage controlvm "vmName" poweroff
        /// </summary>
        public static string StopVM(string vmName)
        {
            if (!IsNameValid(vmName)) return InvalidNameMessage(vmName);

            string state = GetVMState(vmName);
            if (state == "poweroff" || state == "aborted" || state == "saved")
                return $"'{vmName}' is already stopped.";

            VBoxResult result = Run($"controlvm \"{vmName}\" poweroff");

            return result.Succeeded
                ? $"'{vmName}' stopped successfully."
                : $"Could not stop '{vmName}'. {result.ErrorSummary}";
        }

        /// <summary>
        /// Suspends the running VM, freezing it in place.
        /// Runs: VBoxManage controlvm "vmName" pause
        /// </summary>
        public static string PauseVM(string vmName)
        {
            if (!IsNameValid(vmName)) return InvalidNameMessage(vmName);

            string state = GetVMState(vmName);
            if (state == "paused")  return $"'{vmName}' is already paused.";
            if (state != "running") return $"'{vmName}' is not running, so it cannot be paused.";

            VBoxResult result = Run($"controlvm \"{vmName}\" pause");

            return result.Succeeded
                ? $"'{vmName}' paused."
                : $"Could not pause '{vmName}'. {result.ErrorSummary}";
        }

        /// <summary>
        /// Resumes a paused VM.
        /// Runs: VBoxManage controlvm "vmName" resume
        /// </summary>
        public static string ResumeVM(string vmName)
        {
            if (!IsNameValid(vmName)) return InvalidNameMessage(vmName);

            string state = GetVMState(vmName);
            if (state == "running") return $"'{vmName}' is already running.";
            if (state != "paused")  return $"'{vmName}' is not paused, so there is nothing to resume.";

            VBoxResult result = Run($"controlvm \"{vmName}\" resume");

            return result.Succeeded
                ? $"'{vmName}' resumed."
                : $"Could not resume '{vmName}'. {result.ErrorSummary}";
        }

        /// <summary>
        /// Reads the VM's current state, e.g. "running", "paused", "poweroff", "saved".
        /// Runs: VBoxManage showvminfo "vmName" --machinereadable and reads the
        /// VMState="..." line out of the output.
        /// Returns "notfound" when the VM does not exist, or "unavailable" when
        /// VBoxManage itself could not be reached.
        /// </summary>
        public static string GetVMState(string vmName)
        {
            if (!IsNameValid(vmName)) return "invalid";

            VBoxResult result = Run($"showvminfo \"{vmName}\" --machinereadable");

            if (!result.Executed) return "unavailable";

            if (!result.Succeeded)
            {
                // VBoxManage says "Could not find a registered machine named ..."
                string combined = (result.StandardOutput + result.StandardError).ToLowerInvariant();
                if (combined.Contains("could not find a registered machine")) return "notfound";
                return "unavailable";
            }

            // The machine-readable output contains a line such as: VMState="running"
            Match match = Regex.Match(result.StandardOutput, "^VMState=\"([^\"]*)\"",
                RegexOptions.Multiline | RegexOptions.IgnoreCase);

            return match.Success ? match.Groups[1].Value : "unknown";
        }

        /// <summary>
        /// Turns a raw VBoxManage state into wording suitable for a status label.
        /// </summary>
        public static string DescribeState(string state)
        {
            switch (state)
            {
                case "running":     return "Running";
                case "paused":      return "Paused";
                case "poweroff":    return "Powered off";
                case "saved":       return "Saved";
                case "aborted":     return "Aborted";
                case "starting":    return "Starting…";
                case "stopping":    return "Stopping…";
                case "notfound":    return "Not found — the VM is not registered in VirtualBox";
                case "unavailable": return "Unavailable — VBoxManage could not be reached";
                case "invalid":     return "Invalid VM name";
                default:            return string.IsNullOrEmpty(state) ? "Unknown" : state;
            }
        }

        // ==================================================================
        // Internals
        // ==================================================================

        /// <summary>Outcome of one VBoxManage invocation.</summary>
        private class VBoxResult
        {
            /// <summary>False when the process could not even be launched.</summary>
            public bool Executed { get; set; }

            public int ExitCode { get; set; }
            public string StandardOutput { get; set; } = "";
            public string StandardError { get; set; } = "";

            /// <summary>VBoxManage reports success as exit code 0.</summary>
            public bool Succeeded => Executed && ExitCode == 0;

            /// <summary>Best available explanation of a failure, for the UI.</summary>
            public string ErrorSummary
            {
                get
                {
                    string text = !string.IsNullOrWhiteSpace(StandardError) ? StandardError : StandardOutput;
                    text = (text ?? "").Trim();

                    if (text.Length == 0) return $"VBoxManage exited with code {ExitCode}.";
                    if (text.Length > 400) text = text.Substring(0, 400) + "…";
                    return text;
                }
            }
        }

        /// <summary>
        /// Runs VBoxManage with the given arguments and captures both output streams.
        /// Never throws: any failure comes back as a VBoxResult with Executed = false.
        /// </summary>
        private static VBoxResult Run(string arguments)
        {
            var result = new VBoxResult();

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = VBoxManagePath,
                    Arguments = arguments,
                    UseShellExecute = false,        // required to redirect the streams
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.Start();

                    // Read both streams before waiting. Doing it the other way round can
                    // deadlock when a stream's buffer fills up while we are still waiting.
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (!process.WaitForExit(CommandTimeoutMs))
                    {
                        try { process.Kill(); } catch { /* already gone */ }

                        result.Executed = false;
                        result.StandardError = $"VBoxManage did not finish within {CommandTimeoutMs / 1000} seconds.";
                        return result;
                    }

                    result.Executed = true;
                    result.ExitCode = process.ExitCode;
                    result.StandardOutput = output;
                    result.StandardError = error;
                }
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // Thrown when VBoxManage.exe cannot be found or cannot be launched.
                result.Executed = false;
                result.StandardError =
                    $"Could not run VBoxManage ({VBoxManagePath}). Check that VirtualBox is installed " +
                    $"and that the VBoxManagePath setting is correct. {ex.Message}";
            }
            catch (Exception ex)
            {
                result.Executed = false;
                result.StandardError = $"Unexpected error running VBoxManage: {ex.Message}";
            }

            return result;
        }

        /// <summary>True when the VM name is safe to place on a command line.</summary>
        private static bool IsNameValid(string vmName)
            => !string.IsNullOrWhiteSpace(vmName) && SafeVmName.IsMatch(vmName);

        private static string InvalidNameMessage(string vmName)
            => $"'{vmName}' is not a valid VM name. Use letters, numbers, spaces, dots, hyphens or underscores.";
    }
}
