using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace InformationRetrievalPrj
{
    public class Parser
    {
        // SqlConnection conn = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = C:\\Users\\Arel\\Desktop\\New folder\\InformationRetrievalPrj\\InformationRetrievalPrj\\App_Data\\Database1.mdf; Integrated Security = True; Connect Timeout = 30");
        SqlConnection conn = new SqlConnection("Data Source = (LocalDB)\\MSSQLLocalDB; AttachDbFilename = C:\\Users\\Arel\\Desktop\\search project\\InformationRetrievalPrj\\InformationRetrievalPrj\\App_Data\\Database1.mdf; Integrated Security = True; Connect Timeout = 30");
        public string query { get; set; }
        string[] stopList = { "end", "the", "in", "of", "to" };
        public Dictionary<int,List<string>> startParser(string query2Parse) // main controller of the parser
        {
            Dictionary<string, string> wordsAfterParse;
            Dictionary<int, List<string>> resultArray;
            query = removeSpaces(query2Parse);
            wordsAfterParse = checkSearch(query);
            resultArray = getSongsWithWordsToPaint(wordsAfterParse);
            resultArray = sortByRanking(resultArray);
            return resultArray;
        }

        string removeSpaces(string queryWithSpaces)
        {
            return System.Text.RegularExpressions.Regex.Replace(queryWithSpaces.Trim(), @"\s+", " "); // make only 1 space
        }
        Dictionary<int, List<string>> sortByRanking(Dictionary<int, List<string>> wordsWithSongs)
        { // return songs by ranking
            var songsWithCount = new Dictionary<int, double>();
            Dictionary<int, List<string>> sortByRanking1 = new Dictionary<int, List<string>>();
            int countSum = 0;
            foreach (KeyValuePair<int, List<string>> list in wordsWithSongs)
            {
                countSum = 0;
                foreach (string word in list.Value)
                {
                    countSum += getWordCountBySong(getWordID(word), list.Key);
                }
                var myKey = 0;
                int prevWordCount = 0, currentWordCount = 0;
                myKey = songsWithCount.FirstOrDefault(x => x.Value == countSum).Key;
                if (myKey != 0)
                {
                    songsWithCount.Add(list.Key, countSum);
                    for (int i = 0; i < list.Value.Count || i < wordsWithSongs[myKey].Count; i++)
                    {
                        prevWordCount = getWordCountBySong(getWordID(list.Value[i]), myKey);
                        currentWordCount = getWordCountBySong(getWordID(list.Value[i]), list.Key);

                        if (prevWordCount == currentWordCount)
                        { // word same count
                            songsWithCount[list.Key] = countSum - 0.1;
                        }
                        else if (prevWordCount > currentWordCount)
                        {
                            songsWithCount[list.Key] = countSum - 0.1;
                            break;
                        }
                        else if (currentWordCount > prevWordCount)  // new song count is greater then before
                        {
                            songsWithCount[list.Key] = countSum + 0.1;
                            break;
                        }
                    }
                }
                else
                {
                    songsWithCount.Add(list.Key, countSum);
                }

            }
            var items = from pair in songsWithCount  // order by wordCount of each song
                        orderby pair.Value descending
                        select pair;
            var resultDict = items.Select(t => new { t.Key, t.Value })  //  convert LINQ to Dictionary
                   .ToDictionary(t => t.Key, t => wordsWithSongs[t.Key]);
            return resultDict;
        }

        public int getWordID(string Name) // return the id of word and 0 if not found
        {
            conn.Open();
            string query = "SELECT * FROM words WHERE NAME=@Name";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@Name", Name);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            int wordID = 0;
            if (reader.Read())
            {
                wordID = reader.GetInt32(0);
                string word = reader.GetString(1);
            }
            reader.Close();
            conn.Close();
            return wordID;
        }

        public int[] getWordInSongs(int wordID)
        {
            int[] resultArray = new int[2];
            int index = 0;
            conn.Open();
            string query = "SELECT * FROM songs_words_conn WHERE WORD_ID=@wordID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@wordID", wordID);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultArray[index++] = reader.GetInt32(1);
            }
            reader.Close();
            conn.Close();
            return resultArray;
        }

        public bool wordExistINSong(int wordID, int songID) // check if word
        {
            conn.Open();
            string query = "SELECT * FROM songs_words_conn WHERE WORD_ID=@wordID AND SONG_ID=@songID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@wordID", wordID);
            cmd.Parameters.AddWithValue("@songID", songID);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                conn.Close();
                reader.Close();
                return true;
            }
            else
            {
                conn.Close();
                reader.Close();
                return false;
            }
        }

        public bool checkQuery(int wordID, int songID)
        {
            int[] resultArray = new int[2];
            conn.Open();
            string query = "SELECT * FROM songs_words_conn WHERE WORD_ID=@wordID AND SONG_ID=@songID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@wordID", wordID);
            cmd.Parameters.AddWithValue("@songID", songID);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            conn.Close();
            return reader.Read() ? true : false;
        }
        public int[] getWordCount(int wordID)
        {
            int[] resultArray = new int[2];
            int index = 0;
            conn.Open();
            string query = "SELECT * FROM songs_words_conn WHERE WORD_ID=@wordID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@wordID", wordID);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                resultArray[index++] = reader.GetInt32(2);
            }
            reader.Close();
            conn.Close();
            return resultArray;
        }

        public int getWordCountBySong(int wordID, int songID)
        {
            int count = 0;
            conn.Open();
            string query = "SELECT * FROM songs_words_conn WHERE WORD_ID=@wordID AND SONG_ID=@songID";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@wordID", wordID);
            cmd.Parameters.AddWithValue("@songID", songID);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                count = reader.GetInt32(2);
            }
            reader.Close();
            conn.Close();
            return count;
        }

        List<int> getAllSongs()
        {
            List<int> songs = new List<int>();
            conn.Open();
            string query = "select* from songs s where s.SONG_ID not in(select * from abort_song_id)";
            SqlCommand cmd = new SqlCommand(query, conn);
            SqlDataReader reader;
            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                songs.Add(reader.GetInt32(0));
            }
            reader.Close();
            conn.Close();
            return songs;
        }

        bool isInStopList(string wordToCheck)
        {
            foreach (string word in stopList)
            {
                if (wordToCheck == word)
                    return true;
            }
            return false;
        }

        Dictionary<int, List<string>> getSongsWithWordsToPaint(Dictionary<string, string> wordsAfterParse)
        {
            Dictionary<int, List<string>> wordsWithSongsToPaint = new Dictionary<int, List<string>>();
            List<string> words;
            List<int> songsIds = getAllSongs();
            bool word = false;
            string key, value;
            string lastOperator = " ", operatorBetweenBrackets = " ";
            bool shouldBreak = false, firstWord = true, addToList = false;
            bool oneWordInOr = false, sentenceBeforeBrackets = true, andNotFlag = false;
            bool orBFlag = false;
            foreach (int songID in songsIds) // loop of all songs
            {
                words = new List<string>();
                words.Clear();
                sentenceBeforeBrackets = true;
                foreach (var pair in wordsAfterParse) // each word inside query
                {
                    word = wordExistINSong(getWordID(pair.Key), songID);
                    key = pair.Key;
                    value = pair.Value;
                    if (lastOperator == "orB" && pair.Value != "orB" && oneWordInOr == false && pair.Key != "or")  // cannot find any word in or with brackets ( x or y ) 
                        shouldBreak = true;
                    switch (pair.Value)
                    {
                        case "andB":
                            if (word == false) // and operator in brackets 
                            {
                                if (operatorBetweenBrackets == "and")
                                {
                                    shouldBreak = true;
                                }
                                else if(operatorBetweenBrackets == "or" && sentenceBeforeBrackets == false)
                                {
                                    words.Clear();
                                }
                                sentenceBeforeBrackets = false;
                            }
                            else if (lastOperator == "andB")
                            {
                                if(andNotFlag == true)
                                {
                                    shouldBreak = true;
                                }
                                else if(sentenceBeforeBrackets == true)
                                {
                                    if (!isInStopList(key))
                                        words.Add(key);
                                }
                            }
                            else if(lastOperator == "andNot") // and with false
                            {
                                shouldBreak = true;
                            }
                            else 
                            {
                                if (!isInStopList(key))
                                    words.Add(key);
                            }
                            lastOperator = "andB";
                            break;

                        case "orNotB":
                            if (!word == false)
                            {
                                oneWordInOr = true;
                            }
                            break;
                        case "andNotB":
                            if (word == true && operatorBetweenBrackets == "and")
                            {
                                shouldBreak = true;
                            }
                            else if (!word == false)
                            {
                                sentenceBeforeBrackets = false;
                                words.Clear();
                            }
                            lastOperator = "andB";
                            break;
                        case "orB":
                            if (word == false)
                            {
                                if(oneWordInOr == false)
                                    orBFlag = true;
                            }
                            else
                            {
                                if (!isInStopList(key))
                                    words.Add(key);
                                oneWordInOr = true;
                            }
                            lastOperator = "orB";

                            break;
                        case "and":
                            if (word == false || sentenceBeforeBrackets == false)
                            {
                                shouldBreak = true;
                            }
                            else
                            {
                                if (firstWord)
                                {
                                    firstWord = false;
                                    if (!isInStopList(key))
                                        words.Add(key);
                                }
                                else
                                   if (!isInStopList(key))
                                    words.Add(key);
                            }
                            lastOperator = "and";
                            break;
                        case "orNot":

                            break;
                        case "andNot":
                            if (!word == false)
                            {
                                shouldBreak = true; 
                            }
                            lastOperator = "andNot";
                            break;
                        case "operator":
                            
                            operatorBetweenBrackets = pair.Key;
                            if (pair.Key == "or")
                            {
                                lastOperator = "or";
                                orBFlag = false;
                                if (sentenceBeforeBrackets == false)
                                {
                                    words.Clear();
                                }
                            }
                            else if (pair.Key == "and")
                            {
                                if (sentenceBeforeBrackets == false)
                                    shouldBreak = true;
                            }
                            else if(pair.Key == "orNot")
                            {
                                addToList = true;
                                shouldBreak = true;
                                break;
                            }
                            else if(pair.Key == "andNot")
                            {
                                andNotFlag = true;
                                lastOperator = "andNot";
                            }
                            else if (!words.Any()) // 
                                shouldBreak = true;
                            break;
                        case "or":
                            if (word == false)
                            {

                            }
                            else
                            {
                                if (!isInStopList(key))
                                    words.Add(key);
                            }
                            lastOperator = "or";
                            break;
                        case "not":

                            break;
                    }
                    if (shouldBreak)
                    {
                        if(addToList == true)
                        {
                            shouldBreak = false;
                        }
                        break;
                    }

                }

                if (words.Any() && !shouldBreak)
                {
                    if(  !(sentenceBeforeBrackets == false && operatorBetweenBrackets == " "  && orBFlag == false))
                    {
                        wordsWithSongsToPaint.Add(songID, words);
                    }
                    
                }
                shouldBreak = orBFlag = oneWordInOr = addToList = andNotFlag =  false ;  // reset for next loop
                firstWord = firstWord = sentenceBeforeBrackets = true;
                orBFlag = false;
                lastOperator = operatorBetweenBrackets = " ";
            }
            return wordsWithSongsToPaint;
        }

        public Dictionary<string, string> checkSearch(string query)
        {
            Dictionary<string, string> wordsToPaint = new Dictionary<string, string>();
            string[] words = query.Split(' ');
            string tempWord = " ";
            bool andFlag = false;
            string lastWord = " ";
            string[] operators = new string[10];
            int index = 0, outFlag = 0;
            bool brackets = false;
            int andFlagOut = 0;
            bool orBFlag = false;
            bool oneWordInOR = false;
            string operationBeforeBrackets = " ";
            bool operatorAfterBrackets = false, wordBefore = false;
            foreach (string word in words)
            {
                tempWord = word;
                if (Regex.IsMatch(word, @"^[a-zA-Z]+$"))    // check if word contains only letters
                    tempWord = word.ToLower();         // lower only words
                switch (tempWord)
                {
                    case "and":
                        wordBefore = false;
                        andFlag = true;
                        operators[index++] = "and";
                        if (operatorAfterBrackets == false)
                        {
                            if (brackets == false)
                                wordsToPaint[lastWord] = "and"; // and flag for last word
                            else
                                wordsToPaint[lastWord] = "andB";
                        }
                        else
                        {
                            operatorAfterBrackets = false;
                            wordsToPaint.Add("and", "operator"); // check if need to add AND operator later
                        }
                        break;
                    case "or":
                        wordBefore = false;
                        operators[index++] = "or";
                        if (operatorAfterBrackets == false)
                        {
                            if (brackets == false)
                                wordsToPaint[lastWord] = "or"; // and flag for last word
                            else
                            {
                                orBFlag = true;
                                operators[index - 1] = "orB";
                                wordsToPaint[lastWord] = "orB";
                            }
                        }
                        else
                        {
                            operatorAfterBrackets = false;
                            wordsToPaint.Add("or", "operator"); // check if need to add AND operator later
                        }
                        break;

                    case "not":
                        if( operators[index-1] == "or")
                        {
                            wordsToPaint.Add("orNot", "operator");
                        }
                        else if (operators[index - 1] == "and" && brackets == false)
                        {
                            wordsToPaint.Add("andNot", "operator");
                        }
                            operators[index++] = "not";
                        wordBefore = false;
                        break;
                    case "(":
                        operationBeforeBrackets = operators[index > 0 ? index - 1 : 0];
                        wordBefore = false;
                        brackets = true;
                        break;

                    case ")":
                        if (!oneWordInOR)
                        {
                            if (operationBeforeBrackets == "and")
                            {
                                if(andFlag == true)
                                {
                                    outFlag = 1;
                                    andFlagOut = 1;
                                }

                            }
                            else if (operationBeforeBrackets == "or")
                            {

                            }
                        }
                        wordBefore = false;
                        brackets = false;
                        operatorAfterBrackets = true;
                        break;
                    default:

                        if (operators[index > 0 ? index - 1 : 0] == "orB")
                        {
                            oneWordInOR = true;
                        }
                        if (getWordID(tempWord) != 0) // found word in database
                        {
                            if (brackets == false)
                            {
                                if (operators[index > 0 ? index - 1 : index] == "not")
                                {
                                    if (operators[index > 1 ? index - 2 : index] == "and")
                                    {
                                        if (!wordsToPaint.ContainsKey(tempWord))
                                            wordsToPaint.Add(tempWord, "andNot");
                                    }
                                    else if (operators[index > 1 ? index - 2 : index] == "or")
                                    {
                                        if (!wordsToPaint.ContainsKey(tempWord))
                                                wordsToPaint.Add(tempWord, "orNot");
                                    }
                                }
                                else
                                {
                                    if (!wordsToPaint.ContainsKey(tempWord))
                                        wordsToPaint.Add(tempWord, andFlag == true ? "and" : "or"); // 0 not, 1 or , 2 and
                                }

                            }
                            else if (brackets == true)
                            {
                                if (operators[index > 0 ? index - 1 : index] == "not")
                                {
                                    if (operators[index > 1 ? index - 2 : index] == "and" && brackets == true)
                                    {
                                        if (!wordsToPaint.ContainsKey(tempWord))
                                            wordsToPaint.Add(tempWord, "andNotB");
                                    }
                                    else if (operators[index > 1 ? index - 2 : index] == "or" && brackets == true)
                                    {
                                        if (!wordsToPaint.ContainsKey(tempWord))
                                            wordsToPaint.Add(tempWord, "orNotB");
                                    }
                                }
                                else
                                {
                                    if (!wordsToPaint.ContainsKey(tempWord))
                                        wordsToPaint.Add(tempWord, andFlag == true ? "andB" : "orB"); // 0 not, 1 or , 2 and
                                }
                            }

                            if (andFlag == true)
                                andFlag = false;
                        }
                        else if (andFlag == true) // and flag on and we dont find a word
                        {
                            andFlag = false;
                            if (brackets == false && operators[index-1] != "not")
                            {
                                andFlagOut = 1;
                                outFlag = 1;
                            }
                            foreach(var pair in wordsToPaint)
                            {
                                if(pair.Value == "andB")
                                {
                                    wordsToPaint.Remove(pair.Key);
                                    break;
                                }
                            }
                                
                        }


                        if (wordBefore == true) // word after word --> "peace love"
                        {
                            andFlag = true;
                            if (brackets == false)
                            {
                                operators[index++] = "and";
                                wordsToPaint[lastWord] = "and";
                                wordsToPaint[tempWord] = "and";
                            }
                            else
                            {
                                operators[index++] = "andB";
                                wordsToPaint[lastWord] = "andB";
                                wordsToPaint[tempWord] = "andB";
                            }
                        }
                        wordBefore = true;
                        lastWord = tempWord;
                        break;
                }
                //if (outFlag == 1 && (orBFlag == true && oneWordInOR == false) && andFlagOut != 1)
                
                if ( ( outFlag == 1 && (orBFlag == true && oneWordInOR == false) )  || andFlagOut == 1)
                {
                    wordsToPaint.Clear();
                    break;
                }
            }
            return wordsToPaint;
        }
    }
}