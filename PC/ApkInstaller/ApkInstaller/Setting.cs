using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ApkInstaller.ServiceCorrespond;

namespace ApkInstaller
{
    public partial class Setting : Form
    {
        public Setting()
        {
            InitializeComponent();
            this.groupMainSetting.Text = ApkDataModel.APKLan("GroupMainSetting");
            this.groupSelectLanguage.Text = ApkDataModel.APKLan("GroupSelectLanguage");
            this.chkSetting_ProgramExit.Text = ApkDataModel.APKLan("CheckProgramExit");
            this.chkSetting_ProgramTask.Text = ApkDataModel.APKLan("CheckProgramTask");
            this.chkSetting_ProgramOther.Text = ApkDataModel.APKLan("CheckProgramOther");
            this.btnOk.Text = ApkDataModel.APKLan("OK");
            this.btnCancel.Text = ApkDataModel.APKLan("Cancel");
            this.label1.Text = ApkDataModel.APKLan("Language");
            this.Text = ApkDataModel.APKLan("Settings");
            this.lblVersion.Text = Program.SOFT_VERNAME;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;

            int nLanguageID = cmbLanguage.SelectedIndex;
            Program.chkProgramExit = chkSetting_ProgramExit.Checked;
            Program.chkProgramTask = chkSetting_ProgramTask.Checked;
            Program.chkProgramOther = chkSetting_ProgramOther.Checked;

            Program.LANGUAGEID = nLanguageID;
            Program.mINIFileManager.SetIniValue(Program.SECTION_NAME, 
                Program.LANGUAGE_KEY, Program.LANGUAGEID.ToString(), Program.INI_FILE_PATH);

            Program.mINIFileManager.SetIniValue(Program.SECTION_NAME, 
                Program.CHECK_PROGRAM_EXIT_KEY, Program.chkProgramExit.ToString(), Program.INI_FILE_PATH);
            Program.mINIFileManager.SetIniValue(Program.SECTION_NAME,
                Program.CHECK_PROGRAM_TASK_KEY, Program.chkProgramTask.ToString(), Program.INI_FILE_PATH);
            Program.mINIFileManager.SetIniValue(Program.SECTION_NAME,
                Program.CHECK_PROGRAM_OTHER_KEY, Program.chkProgramOther.ToString(), Program.INI_FILE_PATH);

            Program.SwitchLanguage();

            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void Setting_Load(object sender, EventArgs e)
        {
            cmbLanguage.SelectedIndex = Program.LANGUAGEID;
            chkSetting_ProgramExit.Checked = Convert.ToBoolean(Program.chkProgramExit);
            chkSetting_ProgramTask.Checked = Convert.ToBoolean(Program.chkProgramTask);
            chkSetting_ProgramOther.Checked = Convert.ToBoolean(Program.chkProgramOther);
        }
    }
}
