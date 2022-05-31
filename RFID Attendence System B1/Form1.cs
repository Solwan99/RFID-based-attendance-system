using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;
using MySql.Data.MySqlClient;


namespace RFID_Attendence_System_B1
{
    public partial class Form1 : Form
    {
        
        private MySqlConnection Connection = new MySqlConnection("server=localhost; user=root; password=; database=rfid_user_data");
        private MySqlCommand MySQLCMD = new MySqlCommand();
        private MySqlDataAdapter MySQLDA = new MySqlDataAdapter();
        private DataTable DT = new DataTable();
        private string Table_Name = "rfid_user_data_table"; // your table name
        private int Data;
        int idrecieved = 0;
        private bool LoadImagesStr = false;
        private string IDRam;
        private string IMG_FileNameInput;
        private string StatusInput = "Save";
        private string SqlCmdSearchstr;
        public static string StrSerialIn;
        private bool GetID = false;
        private bool ViewUserData = false;

        SerialPort port = null;
        string data_rx = "";
        bool flag_st = false;
        public Form1()
        {
            InitializeComponent();
            refresh_com();
        }
        private void Form1_Load(object sender, EventArgs e)
        {

            PanelRegisterationandEditUserData.Visible = false;
            paneluserdata.Visible = false;
            panelconn.Visible = true;

        }
        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void connectionbutton_Click(object sender, EventArgs e)
        {
            pictureBox2.Top = connectionbutton.Top;
            PanelRegisterationandEditUserData.Visible = false;
            paneluserdata.Visible = false;
            panelconn.Visible = true;
        }

        private void userdatabutton_Click(object sender, EventArgs e)
        {
            if (TimerSerialIn.Enabled == false)
            {
                MessageBox.Show("Failed to open User Data !!!Click the Connection menu then click the Connect button.", "Information");
                MessageBox.Show("Connection failed !!!Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            else
            { 
                StrSerialIn = "";
                ViewUserData = true;
                pictureBox2.Top = userdatabutton.Top;
                PanelRegisterationandEditUserData.Visible = false;
                paneluserdata.Visible = true;
                panelconn.Visible = false;
               // ShowDataUser();
           }
        }

        private void regbutton_Click(object sender, EventArgs e)
        {
            StrSerialIn = "";
            ViewUserData = false;



            pictureBox2.Top = regbutton.Top;
            PanelRegisterationandEditUserData.Visible = true;
            paneluserdata.Visible = false;
            panelconn.Visible = false;



            ShowData();
        }
        private void connect()
        {
            port = new SerialPort(comboBox1.SelectedItem.ToString());
          //  port.DataReceived += new SerialDataReceivedEventHandler(TimerSerialIn_Tick);

            port.BaudRate = 9600;
            port.DataBits = 8;
            port.StopBits = StopBits.One;

            try
            {
                if (!port.IsOpen)
                {
                    port.Open();
                    MessageBox.Show("Open");
                    TimerSerialIn.Start();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void disconnect()
        {
            try
            {
                if (port.IsOpen)
                {
                    port.Close();
                    MessageBox.Show("Close");
                    label1connstatus.Text = "Connection Stauts : Disconnected";
                    label1connstatus.ForeColor = Color.Red;
                    TimerSerialIn.Stop();
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void refresh_com()
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
        }
        private void DataReceiveHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = "";
            try
            {
                indata = sp.ReadExisting();
            }
            catch (Exception E) { }
            int idx_end = indata.IndexOf(';');
            if ((idx_end >= 0) && (flag_st == true))
            {
                flag_st = false;
                data_rx += indata.Substring(0, idx_end);
            }

            int idx_start = indata.IndexOf('@');
            if (idx_start >= 0)
            {
                flag_st = true;
                data_rx = "";
                if (indata.Length > (idx_start + 1))
                {
                    data_rx += indata.Substring((idx_start + 1), (indata.Length - 1));
                    int idx = data_rx.IndexOf(';');
                    if (idx >= 0)
                    {
                        data_rx = data_rx.Substring(0, idx);
                    }
                }
            }
            if (flag_st)
            {
                data_rx += indata;
            }





        }
        private void ShowData()
        {
            try
            { Connection.Open();
            }
            catch (Exception ex)
            { MessageBox.Show("Connection failed !!!Please888check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (LoadImagesStr == false)
                {
                    MySQLCMD.CommandType = CommandType.Text;
                    MySQLCMD.CommandText = "SELECT * FROM " + Table_Name + " WHERE ID LIKE '" + labelid.Text.Substring(5, labelid.Text.Length - 5) + "'";
                    MySQLDA = new MySqlDataAdapter(MySQLCMD.CommandText, Connection);
                    DT = new DataTable();
                    Data = MySQLDA.Fill(DT);
                    
                    if (Data > 0)
                    {
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = DT;
                        dataGridView1.SelectedColumns[2].DefaultCellStyle.Format = "c";
                        dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
                        dataGridView1.ClearSelection();
                    }
                    else
                    { dataGridView1.DataSource = DT; }

                }
                else
                {
                    MySQLCMD.CommandType = CommandType.Text;
                    MySQLCMD.CommandText = "SELECT Images FROM " + Table_Name + " WHERE ID LIKE '" + IDRam + "'";
                    MySQLDA = new MySqlDataAdapter(MySQLCMD.CommandText, Connection);
                    DT = new DataTable();
                    Data = MySQLDA.Fill(DT);
                    
                    
                    DataRow row = DT.Rows[0];
                    
                    if (Data > 0)
                    {
                       
                            byte[] ImgArray = (byte[])row["Images"];
                               
                            var lmgStr = new MemoryStream(ImgArray);
                            PictureBoxImagePreview.Image = Image.FromStream(lmgStr);
                            PictureBoxImagePreview.SizeMode = PictureBoxSizeMode.Zoom;
                            lmgStr.Close();
                        //}
                    }

                    LoadImagesStr = false;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Database !!!" + ex.Message, "Error Message");
                Connection.Close();
                return;
            }



            DT = null;
            Connection.Close();
        }

        private void ShowDataUser()
        {
            try
            {
                Connection.Open();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Show data userConnection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
               // idrecieved = Int32.Parse(StrSerialIn);
                MySQLCMD.CommandType = CommandType.Text;
                MySQLCMD.CommandText = "SELECT * FROM " + Table_Name + " WHERE ID LIKE '"  + labelid.Text.Substring(5,labelid.Text.Length-5) + "'";
                MySQLDA = new MySqlDataAdapter(MySQLCMD.CommandText, Connection);
                DT = new DataTable();
                Data = MySQLDA.Fill(DT);
                if (Data > 0)
                {
                    byte[] ImgArray = (byte[])DT.Rows[0]["Images"];
                    MemoryStream lmgStr = new MemoryStream(ImgArray);

                    PictureBoxImagePreview.Image = Image.FromStream(lmgStr);

                    lmgStr.Close();

                    labelid.Text = DT.Rows[0]["ID"].ToString();
                    LabelName.Text = DT.Rows[0]["Name"].ToString();
                    LabelAddress.Text = DT.Rows[0]["Address"].ToString();
                    LabelCity.Text = DT.Rows[0]["City"].ToString();
                    LabelCountry.Text = DT.Rows[0]["Country"].ToString();
                }

                else
                {
                    MessageBox.Show("ID not found !!! Please register your ID.", "Information Message");
                }


            }

            catch (Exception ex)
            {
                MessageBox.Show("Show data user Failed to load Database !!!" + ex.Message, "Error Message");
                Connection.Close();
                return;
            }



            DT = null;
            Connection.Close();
        }

        private void ClearInputUpdateData()
        {
            TextBoxName.Text = "";
            LabelGetID.Text = "________";
            TextBoxAddress.Text = "";
            TextBoxCity.Text = "";
            TextBoxCountry.Text = "";
            PictureBoxImageInput.Image = Properties.Resources.rsz_1click_here_button_with_cursor_icon_vector_23394642;
        }


        /*private void data_rx_handler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string temp = sp.ReadExisting();
            if (temp.Contains("@"))
            {
                data_rx = "";
                flag_st = true;
            }
            else if (temp.Contains(";"))
            {
                flag_st = false;
                MessageBox.Show(data_rx);

            }
            else if (temp.Contains("%"))
            {
                //whatever.
            }
            if (flag_st)
            {
                data_rx += temp;
            }
        } 
        */

        private void refreshbutton_Click(object sender, EventArgs e)
        {
            refresh_com();
        }

        private void connectbutton_Click(object sender, EventArgs e)
        {
            connect();
        }

        private void disconnectb_Click(object sender, EventArgs e)
        {
            disconnect();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void PanelRegisterationandEditUserData_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void panelconn_Paint(object sender, PaintEventArgs e)
        {

        }

        private void ButtonSave_Click(object sender, EventArgs e)
        {


            byte[] arrImage;
            MemoryStream mstream = new MemoryStream();

            if (TextBoxName.Text == "")

            {
                MessageBox.Show("Name cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (TextBoxAddress.Text == "")
            {
                MessageBox.Show("Address cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (TextBoxCity.Text == "")
            {
                MessageBox.Show("City cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (TextBoxCountry.Text == "")
            {
                MessageBox.Show("Country cannot be empty !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }



            if (StatusInput == "Save")
            {
                if (IMG_FileNameInput != "")
                {
                    PictureBoxImageInput.Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    arrImage = mstream.GetBuffer();
                }
                else
                {
                    MessageBox.Show("The image has not been selected !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    Connection.Open();
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Connection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    MySQLCMD = new MySqlCommand();
                    {
                        var withBlock = MySQLCMD;
                        withBlock.CommandText = "INSERT INTO " + Table_Name + " (Name, ID, Address, City, Country, Images) VALUES (@name, @ID, @address, @city, @country, @images)";
                        withBlock.Connection = Connection;
                        withBlock.Parameters.AddWithValue("@name", TextBoxName.Text);
                        withBlock.Parameters.AddWithValue("@id", LabelGetID.Text);
                        withBlock.Parameters.AddWithValue("@address", TextBoxAddress.Text);
                        withBlock.Parameters.AddWithValue("@city", TextBoxCity.Text);
                        withBlock.Parameters.AddWithValue("@country", TextBoxCountry.Text);
                        withBlock.Parameters.AddWithValue("@images", arrImage);
                        withBlock.ExecuteNonQuery();
                    }

                    MessageBox.Show("Data saved successfully", "Information");
                    IMG_FileNameInput = "";
                    ClearInputUpdateData();
                }

                catch (Exception ex)
                {
                    MessageBox.Show("Data failed to save !!!" + ex.Message, "Error Message");
                    Connection.Close();
                    return;
                }


                Connection.Close();
            }

            else
            {
                if (IMG_FileNameInput != "")
                {
                    PictureBoxImageInput.Image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    arrImage = mstream.GetBuffer();



                    try
                    {
                        Connection.Open();
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    try
                    {
                        MySQLCMD = new MySqlCommand();
                        {
                            var withBlock = MySQLCMD;
                            withBlock.CommandText = "UPDATE " + Table_Name + " SET  Name=@name,ID=@id,Address=@address,City=@city,Country=@country,Images=@images WHERE ID=@id ";
                            withBlock.Connection = Connection;
                            withBlock.Parameters.AddWithValue("@name", TextBoxName.Text);
                            withBlock.Parameters.AddWithValue("@id", LabelGetID.Text);
                            withBlock.Parameters.AddWithValue("@address", TextBoxAddress.Text);
                            withBlock.Parameters.AddWithValue("@city", TextBoxCity.Text);
                            withBlock.Parameters.AddWithValue("@country", TextBoxCountry.Text);
                            withBlock.Parameters.AddWithValue("@images", arrImage);
                            withBlock.ExecuteNonQuery();
                        }

                        MessageBox.Show("Data updated successfully", "Information");
                        IMG_FileNameInput = "";
                        ButtonSave.Text = "Save";
                        ClearInputUpdateData();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Data failed to Update !!!" + ex.Message, "Error Message");
                        Connection.Close();
                        return;
                    }


                    Connection.Close();
                }
                else
                {
                    try
                    {
                        Connection.Open();
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Connection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }


                    try
                    {
                        MySQLCMD = new MySqlCommand();
                        {
                            var withBlock = MySQLCMD;
                            withBlock.CommandText = "UPDATE " + Table_Name + " SET  Name=@name,ID=@id,Address=@address,City=@city,Country=@country WHERE ID=@id ";
                            withBlock.Connection = Connection;
                            withBlock.Parameters.AddWithValue("@name", TextBoxName.Text);
                            withBlock.Parameters.AddWithValue("@id", LabelGetID.Text);
                            withBlock.Parameters.AddWithValue("@address", TextBoxAddress.Text);
                            withBlock.Parameters.AddWithValue("@city", TextBoxCity.Text);
                            withBlock.Parameters.AddWithValue("@country", TextBoxCountry.Text);
                            withBlock.ExecuteNonQuery();
                        }

                        MessageBox.Show("Data updated successfully", "Information");
                        ButtonSave.Text = "Save";
                        ClearInputUpdateData();
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show("Data failed to Update !!!" + ex.Message, "Error Message");
                        Connection.Close();
                        return;
                    }

                    Connection.Close();
                }



                StatusInput = "Save";
            }
            PictureBoxImagePreview.Image = null;
            ShowData();
        }

        private void buttonclear_Click(object sender, EventArgs e)
        {
            labelid.Text = "ID : ________";
            LabelName.Text = "Waiting...";
            LabelAddress.Text = "Waiting...";
            LabelCity.Text = "Waiting...";
            LabelCountry.Text = "Waiting...";
            PictureBoxImagePreview.Image = null;
        }

        private void LabelName_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void labelid_Click(object sender, EventArgs e)
        {

        }

        private void ButtonClearForm_Click(object sender, EventArgs e)
        {
            ClearInputUpdateData();
        }

        private void ButtonScanID_Click(object sender, EventArgs e)
        {
            if (TimerSerialIn.Enabled == true)
            {
                PanelReadingTagProcess.Visible = true;
                GetID = true;
                ButtonScanID.Enabled = false;
            }

            else
            {
                MessageBox.Show("Failed to open User Data !!! Click the Connection menu then click the Connect button.", "Error Message");
            }

        }

        private void PictureBoxImageInput_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenFileDialog1 = new OpenFileDialog();
            OpenFileDialog1.FileName = "";
            OpenFileDialog1.Filter = "JPEG (*.jpeg;*.jpg)|*.jpeg;*.jpg";

            if (OpenFileDialog1.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
            {
                IMG_FileNameInput = OpenFileDialog1.FileName;
                PictureBoxImageInput.ImageLocation = IMG_FileNameInput;
            }

        }

        private void CheckBoxByName_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxByName.Checked == true)
            {
                CheckBoxByID.Checked = false;
            }
            if (CheckBoxByName.Checked == false)
            {
                CheckBoxByID.Checked = true;
            }

        }

        private void CheckBoxByID_CheckedChanged(object sender, EventArgs e)
        {
            if (CheckBoxByID.Checked == true)
            {
                CheckBoxByName.Checked = false;
            }

            if (CheckBoxByID.Checked == false)
            {
                CheckBoxByName.Checked = true;
            }

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void TextBoxSearch_TextChanged(object sender, EventArgs e)
        {
            if (CheckBoxByID.Checked == true)
            {
                if (TextBoxSearch.Text == null)
                {
                    SqlCmdSearchstr = "SELECT Name, ID, Address, City, Country FROM " + Table_Name + " ORDER BY Name";
                }

                else
                {
                    SqlCmdSearchstr = "SELECT Name, ID, Address, City, Country FROM " + Table_Name + " WHERE ID LIKE'" + TextBoxSearch.Text + "%'";
                }
            }
            if (CheckBoxByName.Checked == true)
            {
                if (TextBoxSearch.Text == null)
                {
                    SqlCmdSearchstr = "SELECT Name, ID, Address, City, Country FROM " + Table_Name + " ORDER BY Name";
                }

                else
                {
                    SqlCmdSearchstr = "SELECT Name, ID, Address, City, Country FROM " + Table_Name + " WHERE Name LIKE'" + TextBoxSearch.Text + "%'";
                }

            }
            try
            {
                Connection.Open();
            }

            catch (Exception ex)
            {
                MessageBox.Show("Connection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                MySQLDA = new MySqlDataAdapter(SqlCmdSearchstr, Connection);
                DT = new DataTable();
                Data = MySQLDA.Fill(DT);
                if (Data > 0)
                {
                    dataGridView1.DataSource = null;
                    dataGridView1.DataSource = DT;
                    dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
                    dataGridView1.ClearSelection();
                }

                else
                {
                    dataGridView1.DataSource = DT;
                }

            }

            catch (Exception ex)
            {
                MessageBox.Show("Failed to search" + ex.Message, "Error Message");
                Connection.Close();
            }
            Connection.Close();

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
                if (AllCellsSelected(dataGridView1) == false)
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        // dataGridView1.CurrentCell = this.songsDataGridView.Columns[e.ColumnIndex];
                        int i;
                        if (e.RowIndex >= 0)
                        {
                            i = dataGridView1.CurrentRow.Index;
                            LoadImagesStr = true;
                            IDRam = dataGridView1.Rows[i].Cells["ID"].Value.ToString();
                            ShowData();
                        }
                    }

                }

            }

            catch (Exception ex)
            {
                return;
            }
        }
        private bool AllCellsSelected(DataGridView dgv)
        {
            bool AllCellsSelectedRet = default;
            AllCellsSelectedRet = dataGridView1.SelectedCells.Count == dataGridView1.RowCount * dataGridView1.Columns.GetColumnCount(DataGridViewElementStates.Visible);
            return AllCellsSelectedRet;
        }


         /*private void TimerTimeDate_Tick(object sender, EventArgs e)
         {
             LabelDateTime.Text = "Time " & DateTime.Now.ToString("HH:mm:ss") & "  Date " & DateTime.Now.ToString("dd MMM, yyyy");
         }*/
        
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataGridView1.RowCount == 0)
            {
                MessageBox.Show("Cannot delete, table data is empty", "Error Message");/* TODO ERROR: Skipped SkippedTokensTrivia */
                return;
            }
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Cannot delete, select the table data to be deleted", "Error Message");
                return;
            }

            try
            {
                Connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Connection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                if (AllCellsSelected(dataGridView1) == true)
                {
                    MySQLCMD.CommandType = CommandType.Text;
                    MySQLCMD.CommandText = "DELETE FROM " + Table_Name;
                    MySQLCMD.Connection = Connection;
                    MySQLCMD.ExecuteNonQuery();
                }
                foreach (DataGridViewRow row in dataGridView1.SelectedRows)
                {
                    if (row.Selected == true)
                    {
                        
                        String Text = row.DataBoundItem.ToString();
                        MySQLCMD.CommandType = CommandType.Text;
                        MySQLCMD.CommandText = "DELETE FROM " + Table_Name + " WHERE ID='" + Text + "'";
                        MySQLCMD.Connection = Connection;
                        MySQLCMD.ExecuteNonQuery();
                    }
                }

            }



            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete" + ex.Message, "Error Message");
                Connection.Close();
            }

            PictureBoxImagePreview.Image = null;
            Connection.Close();
            ShowData();
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.SelectAll();
        }

        private void clearSelectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.ClearSelection();
            PictureBoxImagePreview.Image = null;
        }

        private void refreshToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            ShowData();
        }

        private void TimerSerialIn_Tick(object sender, EventArgs e)
        {
            try
            {
                StrSerialIn = port.ReadExisting();
                //idrecieved = Int32.Parse(StrSerialIn);
                label1connstatus.Text = "Connection Status : Connected";
                label1connstatus.ForeColor = Color.Green;
                if (StrSerialIn != "")
                {
                    if (GetID == true)
                    {
                        LabelGetID.Text = StrSerialIn;
                        GetID = false;
                        if (LabelGetID.Text != "________")
                        {
                            PanelReadingTagProcess.Visible = false;
                            IDCheck();
                        }
                    }

                    if (ViewUserData == true)
                    {
                        ViewData();
                    }
                }
            }
            catch (Exception ex)
            {
                TimerSerialIn.Stop();
                disconnect();
                label1connstatus.Text = "Connection Status : Disconnect";
                //PictureBoxStatusConnect.Image = global::My.Resources.Disconnect;
                MessageBox.Show("Failed to connect !!!Microcontroller is not detected.", "Error Message");
                connectbutton_Click(sender, e);
                return;
            }
        }
        private void IDCheck()
        {
            try
            {
                Connection.Open();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ID CHeck Connection failed !!! Please check that the server is ready !!!", "Error Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                MySQLCMD.CommandType = CommandType.Text;
                MySQLCMD.CommandText = "SELECT * FROM " + Table_Name + " WHERE ID LIKE '" + LabelGetID.Text + "'";
                MySQLDA = new MySqlDataAdapter(MySQLCMD.CommandText, Connection);
                DT = new DataTable();
                Data = MySQLDA.Fill(DT);
                if (Data > 0)
                {
                    if (MessageBox.Show("ID registered ! Do you want to edit the data ?", "Confirmation", MessageBoxButtons.OKCancel,MessageBoxIcon.Question) == DialogResult.Cancel)
                    {
                        DT = null;
                        Connection.Close();
                        ButtonScanID.Enabled = true;
                        GetID = false;
                        LabelGetID.Text = "________";
                        return;
                    }
                    else
                    {
                        byte[] ImgArray = (byte[])DT.Rows[0]["Images"];
                        var lmgStr = new MemoryStream(ImgArray);
                        PictureBoxImageInput.Image = Image.FromStream(lmgStr);
                        PictureBoxImageInput.SizeMode = PictureBoxSizeMode.Zoom;
                        TextBoxName.Text = DT.Rows[0]["ID"].ToString();
                        
                        TextBoxAddress.Text = DT.Rows[0]["Address"].ToString();
                        TextBoxCity.Text = DT.Rows[0]["City"].ToString();
                        TextBoxCountry.Text = DT.Rows[0]["Country"].ToString();
                        StatusInput = "Update";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load Database !!!" + ex.Message, "Error Message");
                Connection.Close();
                return;
            }

            DT = null;
            Connection.Close();
            ButtonScanID.Enabled = true;
            GetID = false;
        }

        private void ViewData()
        {
            labelid.Text = "ID : " + StrSerialIn;
            if (labelid.Text == "ID : ________")
            {
                ViewData();
            }
            else
            {
                ShowDataUser();
            }
        }

        private void ButtonCloseReadingTag_Click(object sender, EventArgs e)
        {
            PanelReadingTagProcess.Visible = false;
            ButtonScanID.Enabled = true;
        }

        private void groupBoxdetaileddata_Enter(object sender, EventArgs e)
        {

        }
    }
}