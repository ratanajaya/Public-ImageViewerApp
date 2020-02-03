using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Drawing.Imaging;
using MetadataEditor.AL;
using System.Diagnostics;

namespace MetadataEditor
{
    public partial class FormMain : Form
    {
        string currentRootFolder; //for folder picker dialog, filled with the previous path of the selected folder
        int currentFileIndex = 0; //For album display
        IAppLogic al;
        AlbumViewModel viewModel;

        #region Initialization
        public FormMain(IAppLogic appLogic) {
            InitializeComponent();
            al = appLogic;

            btnNextPage.Parent = pctCover;
            btnPrevPage.Parent = pctCover;
            lbPage.Parent = pctCover;
            pctCover.MouseWheel += PctCover_MouseWheel;
        }
        
        private void FormMain_Load(object sender, EventArgs e) {
            cbTags.DataSource = al.GetTags();
            //cbCategory.DataSource = appLogic.GetCategories();
            //cbOrientation.DataSource = appLogic.GetOrientations();
        }
        #endregion

        #region Browse folder for album
        private async void BtnBrowse_Click(object sender, EventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = currentRootFolder;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                string fullPath = dialog.FileName;
                currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
                viewModel = await al.GetAlbumViewModelAsync(fullPath);
                DisplayAlbum();
            }
        }
        
        #endregion
        
        #region Display Album Infos
        void DisplayAlbum() {
            ClearControls();

            txtTitle.Text = viewModel.Album.Title;
            txtArtists.Text = String.Join(",", viewModel.Album.Artists);
            txtTags.Text = viewModel.Album.GetAllTags();
            txtFlags.Text = viewModel.Album.GetAllFlags();

            foreach (string lan in viewModel.Album.Languages) {
                string controlName = "chkLan" + lan;
                ((CheckBox)this.Controls.Find(controlName, true)[0]).Checked = true;
            }
            SetRadioButton("rbCat", viewModel.Album.Category);
            txtTier.Text = viewModel.Album.Tier.ToString();
            SetRadioButton("rbOri", viewModel.Album.Orientation);
            chkIsWipTrue.Checked = viewModel.Album.IsWip;
            chkIsReadTrue.Checked = viewModel.Album.IsRead;

            currentFileIndex = 0;
            
            using (Bitmap bm = new Bitmap(viewModel.AlbumFiles[currentFileIndex])) {
                pctCover.Image = new Bitmap(bm);//copy bitmap so it wont lock the file
            }
            pctCover.SizeMode = PictureBoxSizeMode.Zoom;

            lbPage.Text = (currentFileIndex + 1) + "/" + viewModel.AlbumFiles.Count;
        }

        void SetRadioButton(string v, string category) {
            RadioButton rb = (RadioButton)this.Controls[0].Controls[v].Controls[v + category];
            rb.Select();
        }

        void ClearControls() {
            foreach (Control control in this.Controls[0].Controls) {
                if (control is TextBox) {
                    TextBox txtbox = (TextBox)control;
                    txtbox.Text = string.Empty;
                }
                else if (control is ListView) {
                    ((ListView)control).Clear();
                }
                else if (control is CheckBox) {
                    CheckBox chkbox = (CheckBox)control;
                    chkbox.Checked = false;
                }
                else if (control is RadioButton) {
                    RadioButton rdbtn = (RadioButton)control;
                    rdbtn.Checked = false;
                }
                else if (control is DateTimePicker) {
                    DateTimePicker dtp = (DateTimePicker)control;
                    dtp.Value = DateTime.Now;
                }
            }
        }

        #endregion

        #region Save #album.json
        private async void BtnCreate_Click(object sender, EventArgs e) {
            viewModel.Album.Title = txtTitle.Text;
            viewModel.Album.Artists = txtArtists.Text.Split(',').ToList();
            viewModel.Album.Category = GetFromRadioButton("rbCat");
            viewModel.Album.Tier = int.Parse(txtTier.Text);
            viewModel.Album.Orientation = GetFromRadioButton("rbOri");
            viewModel.Album.Tags = txtTags.Text.Split(',').ToList();
            viewModel.Album.Flags = txtFlags.Text.Split(',').ToList();
            viewModel.Album.Languages = GetLansFromForm();
            viewModel.Album.IsWip = chkIsWipTrue.Checked;
            viewModel.Album.IsRead = chkIsReadTrue.Checked;

            viewModel.Album.Artists.Sort();
            viewModel.Album.Languages.Sort();
            viewModel.Album.Tags.Sort();

            string retval = await al.SaveAlbumJson(viewModel);
        }

        private string GetFromRadioButton(string v) {
            foreach (Control c in this.Controls[0].Controls[v].Controls) {
                if (((RadioButton)c).Checked) {
                    return c.Text;
                }
            }
            return "";
        }

        List<string> GetLansFromForm() {
            List<string> result = new List<string>();

            foreach (Control c in this.Controls[0].Controls) {
                if (c.Name.Contains("chkLan")) {
                    if (((CheckBox)c).Checked) {
                        result.Add(c.Text);
                    }
                }
            }

            return result;
        }
        #endregion

        #region Next/Prev
        private async void BtnNext_Click(object sender, EventArgs e) {
            List<string> allFolders = Directory.GetDirectories(currentRootFolder).ToList();
            try {
                string fullPath = allFolders[allFolders.IndexOf(viewModel.Path) + 1];
                currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
                viewModel = await al.GetAlbumViewModelAsync(fullPath);
                DisplayAlbum();
            }
            catch(Exception ex) {
            }
        }

        private async void BtnPrev_Click(object sender, EventArgs e) {
            List<string> allFolders = Directory.GetDirectories(currentRootFolder).ToList();
            try {
                string fullPath = allFolders[allFolders.IndexOf(viewModel.Path) - 1];
                currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
                viewModel = await al.GetAlbumViewModelAsync(fullPath);
                DisplayAlbum();
            }
            catch (Exception ex) {
            }
        }

        private void BtnPrevPage_Click(object sender, EventArgs e) {
            PrevPage();
        }

        private void BtnNextPage_Click(object sender, EventArgs e) {
            NextPage();
        }

        private void PctCover_Hover(object sender, EventArgs e) {
            pctCover.Focus();
        }

        private void PctCover_MouseWheel(object sender, MouseEventArgs e) {
            if(e.Delta > 0) {
                PrevPage();
            }
            else {
                NextPage();
            }
        }

        void NextPage() {
            try {
                pctCover.Image = new Bitmap(viewModel.AlbumFiles[++currentFileIndex]);
                lbPage.Text = (currentFileIndex + 1) + "/" + viewModel.AlbumFiles.Count;
            }
            catch (ArgumentOutOfRangeException) {
                currentFileIndex = 0;
                pctCover.Image = new Bitmap(viewModel.AlbumFiles[currentFileIndex]);
                lbPage.Text = (currentFileIndex + 1) + "/" + viewModel.AlbumFiles.Count;
            }
            catch (NullReferenceException) { }
        }

        void PrevPage() {
            try {
                pctCover.Image = new Bitmap(viewModel.AlbumFiles[--currentFileIndex]);
                lbPage.Text = (currentFileIndex + 1) + "/" + viewModel.AlbumFiles.Count;
            }
            catch (ArgumentOutOfRangeException) {
                currentFileIndex = viewModel.AlbumFiles.Count - 1;
                pctCover.Image = new Bitmap(viewModel.AlbumFiles[currentFileIndex]);
                lbPage.Text = (currentFileIndex + 1) + "/" + viewModel.AlbumFiles.Count;
            }
            catch (NullReferenceException) { }
        }

        #endregion

        #region Other UI Actions
        private void CbTags_SelectedIndexChanged(object sender, EventArgs e) {
            txtTags.Text = txtTags.Text.Trim();
            List<string> tags = txtTags.Text.Split(',').ToList();
            tags.Remove("");
            if (tags.Contains(cbTags.SelectedItem)){ //Remove if exist
                tags.Remove(cbTags.SelectedItem.ToString());
            }
            else { //Add if not exist
                tags.Add(cbTags.SelectedItem.ToString());
            }
            txtTags.Text = string.Join(",", tags);
        }

        private void btnExplore_Click(object sender, EventArgs e) {
            try {
                string fileName = viewModel.Path;
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "explorer.exe";
                startInfo.Arguments = "\"" + fileName + "\"";
                Process.Start(startInfo);
            }
            catch(Exception ex) {
            }
        }
        #endregion
    }
}
