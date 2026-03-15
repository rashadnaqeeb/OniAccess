using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Web.Script.Serialization;

static class Program
{
    const string WorkshopUrl = "https://steamcommunity.com/sharedfiles/filedetails/?id=3683507975";
    const string SteamLaunchUrl = "steam://rungameid/457140";
    static readonly string RelativePath = Path.Combine("Klei", "OxygenNotIncluded", "mods", "mods.json");

    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        while (true)
        {
            string path = FindModsJson();
            if (path == null)
            {
                var action = ShowNotFoundDialog();
                if (action == NotFoundAction.OpenWorkshop)
                {
                    Process.Start(WorkshopUrl);
                    continue;
                }
                if (action == NotFoundAction.TryAgain)
                    continue;
                return;
            }

            try
            {
                if (!EnableMod(path))
                {
                    var action = ShowNotFoundDialog();
                    if (action == NotFoundAction.OpenWorkshop)
                    {
                        Process.Start(WorkshopUrl);
                        continue;
                    }
                    if (action == NotFoundAction.TryAgain)
                        continue;
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to update mod config: " + ex.Message,
                    "OniAccess",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                "OniAccess enabled. Launch the game?",
                "OniAccess",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            if (result == DialogResult.Yes)
                Process.Start(SteamLaunchUrl);
            return;
        }
    }

    static string FindModsJson()
    {
        // 1. Shell folder API (handles OneDrive redirects)
        string docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (!string.IsNullOrEmpty(docs))
        {
            string path = Path.Combine(docs, RelativePath);
            if (File.Exists(path))
                return path;
        }

        // 2. %USERPROFILE%\Documents fallback
        string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile))
        {
            string path = Path.Combine(userProfile, "Documents", RelativePath);
            if (File.Exists(path) && path != Path.Combine(docs ?? "", RelativePath))
                return path;
        }

        // 3. Scan fixed drives for Users\*\Documents
        foreach (var drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType != DriveType.Fixed || !drive.IsReady)
                continue;
            string usersDir = Path.Combine(drive.RootDirectory.FullName, "Users");
            if (!Directory.Exists(usersDir))
                continue;
            try
            {
                foreach (string userDir in Directory.GetDirectories(usersDir))
                {
                    string path = Path.Combine(userDir, "Documents", RelativePath);
                    if (File.Exists(path))
                        return path;
                }
            }
            catch (UnauthorizedAccessException) { }
        }

        return null;
    }

    static bool EnableMod(string modsJsonPath)
    {
        var serializer = new JavaScriptSerializer();
        string json = File.ReadAllText(modsJsonPath);
        var root = serializer.Deserialize<Dictionary<string, object>>(json);

        if (!root.ContainsKey("mods") || !(root["mods"] is ArrayList mods))
            return false;

        foreach (Dictionary<string, object> mod in mods.OfType<Dictionary<string, object>>())
        {
            string staticId = mod.ContainsKey("staticID") ? mod["staticID"] as string : null;
            string labelId = null;
            if (mod.ContainsKey("label") && mod["label"] is Dictionary<string, object> label && label.ContainsKey("id"))
                labelId = label["id"] as string;

            if (staticId == "OniAccess" || labelId == "OniAccess")
            {
                mod["enabled"] = true;
                mod["enabledForDlc"] = new ArrayList { "", "EXPANSION1_ID" };
                mod["crash_count"] = 0;
                mod["status"] = 1;

                string output = serializer.Serialize(root);
                File.WriteAllText(modsJsonPath, output);
                return true;
            }
        }

        return false;
    }

    enum NotFoundAction { OpenWorkshop, TryAgain, Close }

    static NotFoundAction ShowNotFoundDialog()
    {
        using (var form = new Form())
        {
            form.Text = "OniAccess";
            form.StartPosition = FormStartPosition.CenterScreen;
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.MaximizeBox = false;
            form.MinimizeBox = false;
            form.ClientSize = new System.Drawing.Size(450, 140);

            var message = new Label
            {
                Text = "Could not find the OniAccess mod config.\n\n" +
                       "If you haven't subscribed yet, click Open Workshop Page.\n" +
                       "If you have subscribed, launch the game once, close it, then click Try Again.",
                Location = new System.Drawing.Point(12, 12),
                Size = new System.Drawing.Size(426, 72),
            };

            var workshopBtn = new Button
            {
                Text = "Open Workshop Page",
                Location = new System.Drawing.Point(12, 96),
                Size = new System.Drawing.Size(140, 30),
            };

            var retryBtn = new Button
            {
                Text = "Try Again",
                Location = new System.Drawing.Point(162, 96),
                Size = new System.Drawing.Size(100, 30),
            };

            var closeBtn = new Button
            {
                Text = "Close",
                Location = new System.Drawing.Point(272, 96),
                Size = new System.Drawing.Size(100, 30),
            };

            var result = NotFoundAction.Close;

            workshopBtn.Click += (s, e) => { result = NotFoundAction.OpenWorkshop; form.Close(); };
            retryBtn.Click += (s, e) => { result = NotFoundAction.TryAgain; form.Close(); };
            closeBtn.Click += (s, e) => { result = NotFoundAction.Close; form.Close(); };

            form.Controls.AddRange(new Control[] { message, workshopBtn, retryBtn, closeBtn });
            form.AcceptButton = retryBtn;
            form.CancelButton = closeBtn;
            form.ShowDialog();

            return result;
        }
    }
}
