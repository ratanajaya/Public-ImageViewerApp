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
using SharedLibrary;

namespace MetadataEditor
{
    public partial class FormMain : Form
    {
        string _currentRootFolder; //for folder picker dialog, filled with the previous path of the selected folder
        int _currentFileIndex = 0; //For album display
        IAppLogic _al;
        IAlbumInfoProvider _ai;

        AlbumViewModel _viewModel;
        AlbumDeleteStatusEnum _deleteStatus;
        List<FileDisplayModel> _fileDisplays;
        string _cachedFolderNext;
        string _cachedFolderPrev;

        Dictionary<string, string> _shortDisplayMap;

        #region Initialization
        public FormMain(IAppLogic appLogic, IAlbumInfoProvider ai) {
            InitializeComponent();

            _shortDisplayMap = new Dictionary<string, string>() {
                { "MG", "Manga" },
                { "CG", "CGSet" },
                { "SC", "SelfComp" },
                { "Ptr", "Portrait" },
                { "Lsc", "Landscape" },
                { "EN", "English" },
                { "JP", "Japanese" },
                { "CH", "Chinese" },
                { "Other", "Other" }
            };

            _al = appLogic;
            _ai = ai;

            btnNextPage.Parent = pctCover;
            btnPrevPage.Parent = pctCover;
            lbPage.Parent = pctCover;
            pctCover.MouseWheel += PctCover_MouseWheel;
            _deleteStatus = AlbumDeleteStatusEnum.NotAllowed;
        }
        
        private void FormMain_Load(object sender, EventArgs e) {
            cbTags.DataSource = _al.GetTags();
            txtTags.Text = "";
            cbTags.DroppedDown = false;
            InitializeEmptyAlbumViewModel();
            //cbCategory.DataSource = appLogic.GetCategories();
            //cbOrientation.DataSource = appLogic.GetOrientations();
        }

        private void InitializeEmptyAlbumViewModel() {
            _viewModel = new AlbumViewModel {
                Album = new Album()
            };
        }
        #endregion

        #region Browse folder for album
        private async void BtnBrowse_Click(object sender, EventArgs e) {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = _currentRootFolder;
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok) {
                string fullPath = dialog.FileName;
                _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
                _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel?.Album);
                _fileDisplays = GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles);
                DisplayAlbum();
            }
        }

        List<FileDisplayModel> GetFileDisplayModels(string albumDir, List<string> filePaths) {
            var result = new List<FileDisplayModel>();
            foreach(string path in filePaths) {
                string fileName = Path.GetFileName(path);
                string subDir = Path.GetDirectoryName(path).Replace(albumDir, "");
                var fileDisplay = new FileDisplayModel {
                    Path = path,
                    FileNameDisplay = fileName,
                    SubDirDisplay = subDir,
                    UploadStatus = "_"
                };
                result.Add(fileDisplay);
            }
            return result;
        }

        #endregion

        #region Display Album Infos
        void DisplayAlbum() {
            ClearControls();

            txtTitle.Text = _viewModel.Album.Title;
            txtArtists.Text = String.Join(",", _viewModel.Album.Artists);
            txtTags.Text = _viewModel.Album.GetAllTags();
            txtFlags.Text = _viewModel.Album.GetAllFlags();

            foreach (string lan in _viewModel.Album.Languages) {
                string controlName = "chkLan" + lan;
                ((CheckBox)this.Controls.Find(controlName, true)[0]).Checked = true;
            }
            SetRadioButton("rbCat", _viewModel.Album.Category);
            txtTier.Text = _viewModel.Album.Tier.ToString();
            SetRadioButton("rbOri", _viewModel.Album.Orientation);
            chkIsWipTrue.Checked = _viewModel.Album.IsWip;
            chkIsReadTrue.Checked = _viewModel.Album.IsRead;

            UpdateFileDisplay();

            _currentFileIndex = 0;

            if(_ai.SuitableVideoFormats.Contains(Path.GetExtension(_viewModel.AlbumFiles[_currentFileIndex]))) {
                //if video show nothing
            }
            else if(_ai.SuitableImageFormats.Contains(Path.GetExtension(_viewModel.AlbumFiles[_currentFileIndex]))) {
                using(Bitmap bm = new Bitmap(_viewModel.AlbumFiles[_currentFileIndex])) {
                    pctCover.Image = new Bitmap(bm);//copy bitmap so it wont lock the file
                }
                pctCover.SizeMode = PictureBoxSizeMode.Zoom;
            }

            lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;

            SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum.NotAllowed);
            _cachedFolderNext = null;
            _cachedFolderPrev = null;
        }

        void UpdateFileDisplay() {
            string[] lines = _fileDisplays.Select(f => f.SubDirDisplay + "|" + f.FileNameDisplay + "|" + f.UploadStatus).ToArray();

            txtFileUpload.Lines = lines;
            txtFileUpload.SelectionStart = txtFileUpload.Text.Length;
            txtFileUpload.ScrollToCaret();
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
            RetrieveAlbumVmValueFromUI();
            string retval = await _al.SaveAlbumJson(_viewModel);
        }

        private void RetrieveAlbumVmValueFromUI() {
            _viewModel.Album.Title = txtTitle.Text;
            _viewModel.Album.Artists = txtArtists.Text.Split(',').ToList();
            _viewModel.Album.Category = GetFromRadioButton("rbCat");
            _viewModel.Album.Tier = int.Parse(txtTier.Text);
            _viewModel.Album.Orientation = GetFromRadioButton("rbOri");
            _viewModel.Album.Tags = txtTags.Text.Split(',').ToList();
            _viewModel.Album.Flags = txtFlags.Text.Split(',').ToList();
            _viewModel.Album.Languages = GetLansFromForm();
            _viewModel.Album.IsWip = chkIsWipTrue.Checked;
            _viewModel.Album.IsRead = chkIsReadTrue.Checked;

            _viewModel.Album.Tags.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            _viewModel.Album.Artists.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            _viewModel.Album.Flags.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            _viewModel.Album.Languages.RemoveAll(s => string.IsNullOrWhiteSpace(s));

            _viewModel.Album.Artists.Sort();
            _viewModel.Album.Languages.Sort();
            _viewModel.Album.Tags.Sort();
        }

        private string GetFromRadioButton(string v) {
            foreach (Control c in this.Controls[0].Controls[v].Controls) {
                if (((RadioButton)c).Checked) {
                    return _shortDisplayMap[c.Text];
                }
            }
            return "";
        }

        List<string> GetLansFromForm() {
            List<string> result = new List<string>();

            foreach (Control c in this.Controls[0].Controls) {
                if (c.Name.Contains("chkLan")) {
                    if (((CheckBox)c).Checked) {
                        result.Add(_shortDisplayMap[c.Text]);
                    }
                }
            }

            return result;
        }
        #endregion

        #region Next/Prev
        string GetRelativeFolder(string currentFolder, int step) {
            string rootFolder = Path.GetDirectoryName(currentFolder);
            string[] allFolders = Directory.GetDirectories(rootFolder);

            int relativeFolderIndex = (Array.IndexOf(allFolders,currentFolder) + step) % allFolders.Length;
            string result = allFolders[relativeFolderIndex];
            return result;
        }

        private async void BtnNext_Click(object sender, EventArgs e) {
            try {
                string fullPath = string.IsNullOrEmpty(_cachedFolderNext) ? GetRelativeFolder(_viewModel.Path, 1) : _cachedFolderNext;
                _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
                _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel?.Album);
                _fileDisplays = GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles);
                DisplayAlbum();
            }
            catch(Exception ex) {
            }
        }

        private async void BtnPrev_Click(object sender, EventArgs e) {
            try {
                string fullPath = string.IsNullOrEmpty(_cachedFolderPrev) ? GetRelativeFolder(_viewModel.Path, -1) : _cachedFolderPrev;
                _currentRootFolder = fullPath.Replace(fullPath.Split('\\').Last(), "");
                _viewModel = await _al.GetAlbumViewModelAsync(fullPath, _viewModel?.Album);
                _fileDisplays = GetFileDisplayModels(_viewModel.Path, _viewModel.AlbumFiles);
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
                using(var bm = new Bitmap(_viewModel.AlbumFiles[++_currentFileIndex])) {
                    pctCover.Image = new Bitmap(bm);
                }
                lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;
            }
            catch (ArgumentOutOfRangeException) {
                _currentFileIndex = 0;
                using(var bm = new Bitmap(_viewModel.AlbumFiles[_currentFileIndex])) {
                    pctCover.Image = new Bitmap(bm);
                }
                lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;
            }
            catch (NullReferenceException) { }
        }

        void PrevPage() {
            try {
                using(var bm = new Bitmap(_viewModel.AlbumFiles[--_currentFileIndex])) {
                    pctCover.Image = new Bitmap(bm);
                }
                lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;
            }
            catch (ArgumentOutOfRangeException) {
                _currentFileIndex = _viewModel.AlbumFiles.Count - 1;
                using(var bm = new Bitmap(_viewModel.AlbumFiles[_currentFileIndex])) {
                    pctCover.Image = new Bitmap(bm);
                }
                lbPage.Text = (_currentFileIndex + 1) + "/" + _viewModel.AlbumFiles.Count;
            }
            catch (NullReferenceException) { }
        }

        #endregion

        #region Other UI Actions

        private void btnTierMin_Click(object sender, EventArgs e) {
            int res;
            int val = int.TryParse(txtTier.Text, out res) ? res : 0;
            txtTier.Text = (val - 1).ToString();
        }

        private void btnTierPlus_Click(object sender, EventArgs e) {
            int res;
            int val = int.TryParse(txtTier.Text, out res) ? res : 0;
            txtTier.Text = (val + 1).ToString();
            chkIsReadTrue.Checked = true;
        }

        public List<string> ChangeSelectedTags(string tag) {
            txtTags.Text = txtTags.Text.Trim();
            List<string> tags = txtTags.Text.Split(new[] { "," }, StringSplitOptions.None).ToList();
            tags.Remove("");
            if(tags.Contains(tag)) { //Remove if exist
                tags.Remove(tag);
            }
            else { //Add if not exist
                tags.Add(tag);
            }
            tags.Sort();
            txtTags.Text = string.Join(",", tags);

            return tags;
        }

        private void CbTags_SelectedIndexChanged(object sender, EventArgs e) {
            cbTags.DroppedDown = true;

            ChangeSelectedTags(cbTags.SelectedItem.ToString());
        }

        private void btnPopupTags_Click(object sender, EventArgs e) {
            FormTags formTags = new FormTags(this, _al.GetTags().ToList(), _viewModel.Album.Tags);
            formTags.StartPosition = FormStartPosition.Manual;
            formTags.ShowInTaskbar = false;
            formTags.ShowIcon = false;
            formTags.ControlBox = false;
            formTags.Text = String.Empty;

            var location = this.Location;
            var marginLeft = btnPopupTags.Left + btnPopupTags.Width;
            var marginTop = btnPopupTags.Top + btnPopupTags.Height;
            location.Offset(marginLeft, marginTop);
            formTags.Location = location;
            formTags.Show(btnPopupTags);
        }

        private void btnExplore_Click(object sender, EventArgs e) {
            try {
                string fileName = _viewModel.Path;
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = "explorer.exe";
                startInfo.Arguments = "\"" + fileName + "\"";
                Process.Start(startInfo);
            }
            catch(Exception ex) {
            }
        }

        private async void btnPost_Click(object sender, EventArgs e) {
            RetrieveAlbumVmValueFromUI();

            var saveTask = _al.SaveAlbumJson(_viewModel);

            var progress = new Progress<FileDisplayModel>(model => {
                var uploadedFIle = _fileDisplays.FirstOrDefault(fd => fd.Path == model.Path);
                uploadedFIle.UploadStatus = model.UploadStatus;

                //txtFileUpload.AppendText($"{uploadedFIle.SubDirDisplay}|{uploadedFIle.FileNameDisplay}|{uploadedFIle.UploadStatus}");
                UpdateFileDisplay();
            });

            var albumId = await _al.PostAlbumJson(_viewModel, progress);
            var saveTaskRetval = await saveTask;

            SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum.Allowed);
            //var confirmResult = MessageBox.Show("Success", albumId, MessageBoxButtons.OK);
        }


        private async void btnPostMetadata_Click(object sender, EventArgs e) {
            RetrieveAlbumVmValueFromUI();
            var albumId = await _al.PostAlbumMetadata(_viewModel);
            var confirmResult = MessageBox.Show(albumId, "Success", MessageBoxButtons.OK);
        }

        private void btnDelete_Click(object sender, EventArgs e) {
            try {
                _cachedFolderNext = GetRelativeFolder(_viewModel.Path, 1);
                _cachedFolderPrev = GetRelativeFolder(_viewModel.Path, -1);

                Directory.Delete(_viewModel.Path, true);

                SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum.Deleted);
            }
            catch(Exception ex) {
                var confirmResult = MessageBox.Show(ex.Message,"Exception", MessageBoxButtons.OK);
            }
        }

        private void SetDeleteStatusAndUIAccordingly(AlbumDeleteStatusEnum deleteStatus) {
            _deleteStatus = deleteStatus;
            bool enableUIGroup = true;

            if(deleteStatus == AlbumDeleteStatusEnum.NotAllowed) {
                enableUIGroup = true;
                btnDelete.Enabled = false;
            }
            else if(deleteStatus == AlbumDeleteStatusEnum.Allowed) {
                enableUIGroup = true;
                btnDelete.Enabled = true;
            }
            else if(deleteStatus == AlbumDeleteStatusEnum.Deleted) {
                enableUIGroup = false;
            }

            btnPost.Enabled = enableUIGroup;
            btnCreate.Enabled = enableUIGroup;
        }

        #endregion
    }
}
