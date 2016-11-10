<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="HelpPage.aspx.cs" Inherits="InformationRetrievalPrj.HelpPage" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>
    <link href="style.css" rel="stylesheet" />
    <title>Help!</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <div id="headerDiv" runat="server">
                <h1 class="helpDiv" style="text-align: center">Help! I Need Somebody!</h1>
            </div>
            <div id="helpSearchDiv" runat="server">
                <ul>
                    <li>
                        <h4>in order to search you have to put a query in the search field and hit for the search button.</h4>
                    </li>
                    <li>
                        <h4>the query contains only words with operators between them and brackets.
                            all the query must be split with a space between each operator/brackets/word.
                        </h4>
                    </li>
                    <li>
                        <h4>simple search query's without operators:
                            love - will return all the documents that have love.
                            peace love - will return all the documents that have love AND peace
                        </h4>
                    </li>
                    <li>
                        <h4>Operators:  you can use operators and brackets for more Sophisticated search.
                            the operators we support is AND, OR, NOT. use AND between 2 words for search documents that contains both words.
                            example: peace AND love
                            you can also search for:
                            peace AND love AND money - will bring all the documents that contains all the 3 words.

                        </h4>
                    </li>
                    <li>
                        <h4>OR Operator:
                            Or operator will make sure at least 1 of the words contains in the document.
                            peace OR love will bring back the documents that contains peace or love or both of the words, but if none of them it will return nothing.

                        </h4>

                    </li>
                    <li>
                        <h4>NOT operator:
                               NOT operator must come after AND or OR operators, its use if you want to get word and not other.
                                peace AND NOT love - will return the documents that contains peace but don't contains love.
                        </h4>

                    </li>   
                    <li>
                        <h4>Brackets 
                            you can use brackets with 1 level in order to search.
                            example:
                            peace or ( love or do ).
                            peace and ( love and do ).
                        </h4>

                    </li>   
                    <li>
                        <h4>Common words - there are some common words that this engine don't search for it because they apply so often, so if you search for the word "end" or "the", the engine will not bring you any document.

                        </h4>

                    </li>     
                    <li>
                        <h4>The result:
                                we made our database on the Beatles songs, after you hit the search button you will get the documents you asked with the query.
                                when you press the song you will see the full song with a print option, the words in your query will be in red.
                        </h4>

                    </li>       
                </ul>

            </div>
        </div>
    </form>
    <footer>
        <p>© Copyright 2016, Yinon Manor & Arel Gindos</p>
    </footer>
</body>
</html>
