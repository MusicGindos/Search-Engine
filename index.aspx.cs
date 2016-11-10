using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace InformationRetrievalPrj
{
    public partial class index : System.Web.UI.Page
    {
        // SqlConnection conn = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = C:\\Users\\Yinon\\Documents\\visual studio 2015\\Projects\\InformationRetrievalPrj\\InformationRetrievalPrj\\App_Data\\Database1.mdf; Integrated Security = True; Connect Timeout = 30");
        SqlConnection conn = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = C:\\Users\\Arel\\Desktop\\search project\\InformationRetrievalPrj\\InformationRetrievalPrj\\App_Data\\Database1.mdf; Integrated Security = True; Connect Timeout = 30");
        Parser parser = new Parser();
        static bool enable = false;
        static string globalWordsToPrint;
        static bool loginFlag = false;
        static Dictionary<int, List<string>> globalResultList = new Dictionary<int, List<string>>();
        List<string> wordsToPaint = new List<string>();
        System.Web.UI.WebControls.Label songWords = new System.Web.UI.WebControls.Label();
        System.Web.UI.WebControls.Label songWords2 = new System.Web.UI.WebControls.Label();
        protected void Page_Load(object sender, EventArgs e)
        {


            if (!Page.IsPostBack)
            {
                PlaceHolder1.Controls.Clear();
                showResult(globalResultList);
            }
            else if (enable)
            {
                PlaceHolder1.Controls.Clear();
                showResult(globalResultList);
            }

            radioButton1.Visible = false;
            returnSongDiv.Visible = false;
            printButton.Visible = false;
            backButton.Visible = false;
            if (!loginFlag)
            {
                removeSongButton.Visible = false;
                returnSongButton.Visible = false;
                uploadButton.Visible = false;
                /* removeSongButton.Enabled = false;
                 returnSongButton.Enabled = false;
                 uploadButton.Enabled = false;*/
            }
        }

        protected void searchButton_Click(object sender, EventArgs e)
        {
            //******************HTML********************
            radioButton.Items.Clear();
            abortRadioButtonList.Items.Clear();
            //******************************************
            if (loginFlag)
            {
                removeSongButton.Visible = true;
                returnSongButton.Visible = true;
                uploadButton.Visible = true;
                removeSongButton.Enabled = true;
                returnSongButton.Enabled = true;
                uploadButton.Enabled = true;
            }

            showResult(parser.startParser(searchText.Value));
        }

        protected void uploadButton_Click(object sender, EventArgs e)
        {
            //******************HTML********************
            radioButton.Items.Clear();
            abortRadioButtonList.Items.Clear();
            //******************************************

            // SqlConnection conn = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = C:\\Users\\Yinon\\Documents\\visual studio 2015\\Projects\\InformationRetrievalPrj\\InformationRetrievalPrj\\App_Data\\Database1.mdf; Integrated Security = True; Connect Timeout = 30");
            conn.Open();
            string path = @"C:\Users\Arel\Desktop\beatles\";
            string targetPath = @"C:\Users\Arel\Desktop\beatlesStoreDirectory\";
            char[] delimiters = { ' ', ',', '.', ':', '\t', '\n', ';', '(', ')', '\r' };
            DirectoryInfo directory = new DirectoryInfo(path);
            FileInfo[] Files = directory.GetFiles("*.txt");
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            int fileCount = 0;
            foreach (FileInfo file in Files)
            {
                fileCount++;
                //string fileName = "";
                string songId = string.Empty;
                string fileName = file.Name;
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName).ToLower();
                string sourceFile = System.IO.Path.Combine(path, fileName);
                string targetFile = System.IO.Path.Combine(targetPath, fileName);
                //if the song exsiste
                if (existeCheck("songs", fileNameWithoutExtension)) continue;

                else
                {
                    //insert song to DB and get the song id

                    string query = "insert into songs Output Inserted.SONG_ID values(@songName)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@songName", fileNameWithoutExtension);
                    SqlDataReader reader;
                    reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        songId = reader["SONG_ID"].ToString();
                    }
                    reader.Close();
                }

                string songText = System.IO.File.ReadAllText(path + fileName);

                string[] words = songText.Split(delimiters);
                foreach (string word in words)
                {
                    if (word == "") continue;
                    string tempWord = word.ToLower();
                    if (dictionary.ContainsKey(tempWord))
                    {
                        dictionary[tempWord] += 1;
                    }
                    else
                    {
                        dictionary.Add(tempWord, 1);
                    }
                }
                insertWords(dictionary, fileNameWithoutExtension, songId);
                dictionary.Clear();

                System.IO.File.Copy(sourceFile, targetFile, true);
                System.IO.File.Delete(sourceFile);
            }
            conn.Close();
            MessageBox.Show("finish upload " + fileCount + " songs");
        }

        private bool existeCheck(string tableName, string Name)
        {
            // SqlConnection conn = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = C:\\Users\\Yinon\\Documents\\visual studio 2015\\Projects\\InformationRetrievalPrj\\InformationRetrievalPrj\\App_Data\\Database1.mdf; Integrated Security = True; Connect Timeout = 30");
            // conn.Open();
            string query = "select * from " + tableName + " where NAME=@Name";
            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Name", Name);

            SqlDataReader reader;
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                reader.Close();
                //conn.Close();
                return true;
            }
            reader.Close();
            // conn.Close();

            return false;
        }



        private void insertWords(Dictionary<string, int> songWords, string songName, string songId)
        {
            foreach (KeyValuePair<string, int> word in songWords)
            {

                string wordId = string.Empty;
                //word existe in DB
                if (existeCheck("words", word.Key))
                {
                    //1.get the word id from DB
                    string query = "select WORD_ID from words where NAME=@Name";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@Name", word.Key);

                    SqlDataReader reader;
                    reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        wordId = reader["WORD_ID"].ToString();
                    }
                    reader.Close();

                    //2.insert the word count into the song words table
                    insertWordCount(songId, wordId, word.Value);

                }
                //word not existe in DB
                else
                {
                    //1.insert word to DB and get the word id

                    string query = "insert into words Output Inserted.WORD_ID values(@wordName)";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@wordName", word.Key);
                    SqlDataReader reader;
                    reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        wordId = reader["WORD_ID"].ToString();
                    }
                    reader.Close();

                    //3.insert the word id and the song id to DB with the word count 
                    insertWordCount(songId, wordId, word.Value);
                }



            }

        }

        private void insertWordCount(string songId, string wordId, int wordCount)
        {
            string query = "insert into songs_words_conn values (@wordId,@songId,@wordCount)";
            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@wordId", wordId);
            cmd.Parameters.AddWithValue("@songId", songId);
            cmd.Parameters.AddWithValue("@wordCount", wordCount);

            cmd.ExecuteNonQuery();
        }

        protected void removeSongButton_Click(object sender, EventArgs e)
        {
            abortRadioButtonList.Items.Clear();
            Dictionary<int, string> songsList = new Dictionary<int, string>();
            //songsList = getAllSongsName();
            songsList = getSongsWithourAborted("not in");
            returnSongButton.Enabled = true;
            removeSongButton.Enabled = true;
            if (songsList.Count == 0)
            {
                MessageBox.Show("no songs to hide");
                return;
            }
            foreach (KeyValuePair<int, string> song in songsList)
            {
                radioButton.Items.Add(new ListItem(song.Value, song.Key.ToString()));
            }
            radioButton1.Visible = true;
            removeSongButton.Enabled = false;


            // getSongsWithourAborted();
        }

        protected void returnSongButton_Click(object sender, EventArgs e)
        {
            radioButton.Items.Clear();
            Dictionary<int, string> songsList = new Dictionary<int, string>();
            songsList = getSongsWithourAborted("in");
            returnSongButton.Enabled = true;
            removeSongButton.Enabled = true;
            if (songsList.Count == 0)
            {
                MessageBox.Show("no songs to return");
                return;
            }
            foreach (KeyValuePair<int, string> song in songsList)
            {
                abortRadioButtonList.Items.Add(new ListItem(song.Value, song.Key.ToString()));
            }
            returnSongDiv.Visible = true;
            returnSongButton.Enabled = false;

        }

        protected void deleteButton_Click(object sender, EventArgs e)
        {

            removeSongButton.Enabled = true;
            if (radioButton.SelectedValue == "")
            {
                radioButton.Items.Clear();
                radioButton1.Visible = false;
                return;
            }
            string songId = radioButton.SelectedValue;
            //insert song id to abort table
            insertToAbort(Convert.ToInt32(songId));

            //abortFileList.Add(Convert.ToInt32(songId));
            //deleteSong(songId, "songs");
            //deleteSong(songId, "songs_words_conn");
            radioButton.Items.Clear();
            radioButton1.Visible = false;
            showResult(parser.startParser(searchText.Value));


        }

        //not use at the moment
        private Dictionary<int, string> getAllSongsName()
        {
            conn.Open();
            Dictionary<int, string> songsList = new Dictionary<int, string>();
            string query = "select * from songs";
            SqlCommand cmd = new SqlCommand(query, conn);

            SqlDataReader reader;
            reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                songsList.Add(Convert.ToInt32(reader["SONG_ID"]), reader["NAME"].ToString());
            }
            reader.Close();
            conn.Close();
            return songsList;
        }


        //* not in use at the moment
        private void deleteSong(string songId, string tableName)
        {
            conn.Open();
            string query = "delete from " + tableName + " where SONG_ID=@songId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@songId", Convert.ToInt32(songId));
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        private int getSongId(string songName)
        {
            int songId = 0;
            conn.Open();
            string query = "select SONG_ID from songs where NAME=@Name";
            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@Name", songName);

            SqlDataReader reader;
            reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                songId = Convert.ToInt32(reader["SONG_ID"]);
            }
            reader.Close();
            conn.Close();
            return songId;
        }

        private Dictionary<int, string> getSongsWithourAborted(string flag)
        {
            Dictionary<int, string> songsList = new Dictionary<int, string>();
            string query;
            conn.Open();
            if (flag == "in")
            {
                query = "select* from songs s where s.SONG_ID in(select * from abort_song_id)";
            }
            else
            {
                query = "select* from songs s where s.SONG_ID not in(select * from abort_song_id)";
            }
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                songsList.Add(Convert.ToInt32(reader["SONG_ID"]), reader["NAME"].ToString());
            }
            reader.Close();
            conn.Close();
            return songsList;
        }
        private void insertToAbort(int songId)
        {
            conn.Open();
            string query = "insert into abort_song_id values (@songId)";
            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@songId", songId);
            cmd.ExecuteNonQuery();



            conn.Close();
        }

        private void deleteFromAbort(int songId)
        {
            conn.Open();
            string query = "delete from abort_song_id where id=@songId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@songId", songId);
            cmd.ExecuteNonQuery();
            conn.Close();
        }

        protected void returnButton_Click(object sender, EventArgs e)
        {

            returnSongDiv.Visible = false;
            returnSongButton.Enabled = true;
            if (abortRadioButtonList.SelectedValue == "")
            {
                abortRadioButtonList.Items.Clear();
                return;
            }
            string songId = abortRadioButtonList.SelectedValue;
            //insert song id to abort table
            deleteFromAbort(Convert.ToInt32(songId));
            abortRadioButtonList.Items.Clear();
            showResult(parser.startParser(searchText.Value));
        }

        //get file from directory
        private string getFileFromDirectory(string songName)
        {
            string filePath = @"C:\Users\Arel\Desktop\beatlesStoreDirectory\" + songName + ".txt";
            return filePath;
        }

        private void showResult(Dictionary<int, List<string>> resultList)
        {
            getWordsToPaint(resultList);
            enable = true;
            globalResultList = resultList;
            createSongLinkButton(resultList);
        }


        private void createSongLinkButton(Dictionary<int, List<string>> resultList)
        {
            char[] delimiters = { ' ', ',', '.', ':', '\t', '\n', ';', '(', ')', '\r' };
            PlaceHolder1.Controls.Clear();
            string songName;
            string line1;
            string line2;
            string songText;


            foreach (KeyValuePair<int, List<string>> song in resultList)
            {
                songName = getSongName(song.Key);
                using (StreamReader reader = new StreamReader(getFileFromDirectory(songName)))
                {
                    line1 = reader.ReadLine();
                    line2 = reader.ReadLine();
                }
                System.Web.UI.WebControls.Label lable = new System.Web.UI.WebControls.Label();
                songText = line1;
                songText += " ";
                songText += line2;
                string[] words = songText.Split(delimiters);
                foreach (string word in words)
                {
                    if (wordsToPaint.Contains(word.ToLower()))
                    {
                        lable.Text += "<span style='color: red;'><b>" + word + "</b></span>" + " ";
                    }
                    else
                    {
                        lable.Text += word + " ";
                    }
                }
                LinkButton lb = new LinkButton();
                System.Web.UI.WebControls.Image image = new System.Web.UI.WebControls.Image();
                image.ImageUrl = "images\\" + songName + ".jpg";
                image.CssClass = "albums";
                //lable.Text = line1 + "</br>" + line2;
                lb.Text = songName;
                lb.ID = song.Key.ToString();
                lb.CommandArgument = Convert.ToString(song.Key);
                lb.CommandName = Convert.ToString(song.Key);
                lb.Click += new EventHandler(this.song_Click);
                this.Controls.Add(lb);
                PlaceHolder1.Controls.Add(lb);
                PlaceHolder1.Controls.Add(new LiteralControl("<br />"));
                PlaceHolder1.Controls.Add(lable);
                PlaceHolder1.Controls.Add(new LiteralControl("<br>"));
                PlaceHolder1.Controls.Add(new LiteralControl("<br>"));
                PlaceHolder1.Controls.Add(image);
            }
        }

        private string getSongName(int songId)
        {
            string songName = string.Empty;
            conn.Open();
            string query = "select NAME from songs where SONG_ID=@songId";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@songId", songId);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                songName = reader["NAME"].ToString();
            }
            reader.Close();
            conn.Close();



            return songName;
        }


        public void showAllSong(string songName)
        {
            songNameLable.Text = songName;
            /*Page.ClientScript.RegisterStartupScript(
            this.GetType(), "OpenWindow", "window.open('http://localhost:41741/showSongs.aspx','_newtab');", true);*/
            char[] delimiters = { ' ', ',', '.', ':', '\t', '\n', ';', '(', ')', '\r' };
            string songText = System.IO.File.ReadAllText(getFileFromDirectory(songName));
            globalWordsToPrint = songText;
            string[] words = songText.Split(delimiters);
            HtmlGenericControl divcontrol = new HtmlGenericControl();
            HtmlGenericControl divcontrolForPrint = new HtmlGenericControl();
            divcontrol.TagName = "div";
            divcontrolForPrint.TagName = "div";
            resultDivText.Controls.Add(divcontrol);
            songDiv.Controls.Add(divcontrolForPrint);


            // System.Web.UI.WebControls.Label songWords = new System.Web.UI.WebControls.Label();
            songWords.Text = "";
            songWords2.Text = "";
            divcontrol.Controls.Add(songWords);
            divcontrolForPrint.Controls.Add(songWords2);
            foreach (string word in words)
            {
                songWords2.Text += word + " ";
                if (wordsToPaint.Contains(word.ToLower()))
                {
                    songWords.Text += "<span style='color: red;'><b>" + word + "</b></span>" + " ";
                }
                else
                {
                    songWords.Text += word + " ";
                }
            }
            printButton.Visible = true;
            backButton.Visible = true;
        }

        protected void song_Click(object sender, EventArgs e)
        {
            PlaceHolder1.Controls.Clear();
            string songName;
            LinkButton btn = (LinkButton)sender;
            songName = btn.Text;
            showAllSong(songName);
        }

        private void getWordsToPaint(Dictionary<int, List<string>> resultList)
        {
            wordsToPaint.Clear();
            foreach (KeyValuePair<int, List<string>> song in resultList)
            {
                foreach (string word in song.Value)
                {
                    if (wordsToPaint.Contains(word))
                    {
                        continue;
                    }
                    wordsToPaint.Add(word);
                }
                // break;
            }
        }

        protected void printButton_Click(object sender, EventArgs e)
        {

            //  Page.ClientScript.RegisterStartupScript(this.GetType(), "CallMyFunction", "printdiv()", true);

        }

        protected void helpButton_Click(object sender, EventArgs e)
        {
            Page.ClientScript.RegisterStartupScript(
            this.GetType(), "OpenWindow", "window.open('http://localhost:41741/HelpPage.aspx','_newtab');", true);

        }

        protected void loginButton_Click(object sender, EventArgs e)
        {
            if (passwordTextBox.Text.ToLower() == "letitbe")
            {
                removeSongButton.Visible = true;
                returnSongButton.Visible = true;
                uploadButton.Visible = true;
                /*removeSongButton.Enabled = true;
                returnSongButton.Enabled = true;
                uploadButton.Enabled = true;*/
                loginFlag = true;
            }
            else
            {
                MessageBox.Show("incorrect password\ntry again");
            }
        }

        protected void logoffButton_Click(object sender, EventArgs e)
        {
            removeSongButton.Visible = false;
            returnSongButton.Visible = false;
            uploadButton.Visible = false;
            /*uploadButton.Enabled = false;
            removeSongButton.Enabled = false;
            returnSongButton.Enabled = false;*/
            loginFlag = false;
            passwordTextBox.Text = "";
        }

        protected void backButton_Click(object sender, EventArgs e)
        {
            showResult(parser.startParser(searchText.Value));
        }
    }
}
