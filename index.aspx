<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="index.aspx.cs" Inherits="InformationRetrievalPrj.index" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" />
    <title>Search Website</title>
    <link href="style.css" rel="stylesheet" />
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/1.12.4/jquery.min.js"></script>

</head>
<body>
          <h1 style="text-align:center;">BITLes Search</h1>
    <form id="form1" runat="server">
        <div>
            <div class="col-lg-10">
               <div class="input-group">
                    <input id="searchText" runat="server" class="searchText form-control" type="text" placeholder="Search for..."/>
                    <span class="input-group-btn">
                        <asp:Button runat="server" value="button" Text="Search" OnClick="searchButton_Click" ID="searchButton" CssClass="btn btn-default btn-primary" />
                    </span>
                </div>
              </div>
        </div>
        <div class="btn-group" role="group" aria-label="...">
            <asp:Button runat="server" value="button" Text="Upload" OnClick="uploadButton_Click" ID="uploadButton" CssClass="btn-primary btn btn-default" />
            <asp:Button ID="helpButton" runat="server" Text="Help" OnClick="helpButton_Click" CssClass="helpButton btn-primary btn btn-default"/>
        </div>




        <div class="btn-group logIn" role="group" aria-label="...">
            <asp:Button runat="server" ID="logoffButton" Text="Logout" OnClick="logoffButton_Click" CssClass="btn-primary btn btn-default" />

            <asp:Button runat="server" ID="loginButton" Text="Login" OnClick="loginButton_Click" CssClass="btn-primary btn btn-default"/>
        </div>
        <div class="password">
            <asp:TextBox runat="server" id="passwordTextBox" placeholder="Admin Password" CssClass="inputPassword"></asp:TextBox>
        </div>
                <div class="btn-group abortButtons" role="group" aria-label="...">
            
            <asp:Button runat="server" UseSubmitBehavior="true" ID="removeSongButton" Text="Abort song" type="button" OnClick="removeSongButton_Click" CssClass="btn-primary btn btn-default"/>
            <asp:Button runat="server" ID="returnSongButton" Text="Return song" OnClick="returnSongButton_Click" CssClass="btn-primary btn btn-default"/>
        </div>
        <div id="radioButton1" runat="server" class="radioButtons">
            <asp:RadioButtonList runat="server" ID="radioButton">
            </asp:RadioButtonList>
            <asp:Button runat="server" ID="deleteButton" Text="Delete!" OnClick="deleteButton_Click" CssClass="btn-danger btn btn-default"/>
        </div>
        <div id="returnSongDiv" runat="server" class="radioButtons">
            <asp:RadioButtonList runat="server" ID="abortRadioButtonList">
            </asp:RadioButtonList>
            <asp:Button runat="server" ID="returnButton" Text="Return!" OnClick="returnButton_Click" CssClass="btn-danger btn btn-default"/>

        </div>
        <div id="resultLinkDiv" runat="server" class="songsResult">
            <%--  <asp:Button runat="server" ID="Button1" Text="temp" OnClick="Button1_Click"/>--%>
            <%--<asp:Button runat="server" ID="Button2" Text="tmep2" OnClick="Button2_Click"/>--%>

            <div id="resultDivText" runat="server" style="width: 450px">
                <div id="printDiv" runat="server">
                    <asp:Button ID="printButton" runat="server" Text="Print" CssClass="btn-success btn btn-default" OnClick="printButton_Click" OnClientClick="return  printdiv()"/>
                     <asp:Button ID="backButton" runat="server" Text="Back" CssClass="btn-success btn btn-default" OnClick="backButton_Click"/>
                </div>
                <asp:PlaceHolder ID="PlaceHolder1" runat="server">
                    
                </asp:PlaceHolder>
                
                
            </div>
            <div id="songDiv" runat="server" style="visibility:hidden">
                <asp:Label ID="songNameLable" runat="server" style="visibility:hidden"></asp:Label>

                </div>
            


        </div>
        <script>
       
        function printdiv(e) {
            //alert("hello print");
            
            var divToPrint = document.getElementById('songDiv');
            var popupWin = window.open('', '_blank', 'width=300,height=400,location=no,left=200px');
            popupWin.document.open();
            popupWin.document.write('<html><title>' + songNameLable.innerHTML + '</title><body onload="window.print()">' + divToPrint.innerHTML + '</html>');
            popupWin.document.close();
            e.preventDefault();
            return false;

        }
    </script>
        <%--<script >
            function func() {
                $(document).ready(function () {
                    var flag = 0;
                    $("#radioButton1").hide();
                    $("#removeSongButton").click(function (e) {
                        e.preventDefault();
                        $("#radioButton1").toggle(800);
                        var songs = ['all you need is love', 'steal my guitar gently weeps'];
                        if (flag == 0) {
                            var radioBtn = $('#radioButtonDynam');
                            for (var i = 0; i < songs.length; i++) {
                                radioBtn.append('<label><input type="radio" name="songs" value="' + songs[i] + '" /> ' + songs[i] + '</label></br>');
                            }
                            flag = 1;
                        }
                    });
                });
            }
            </script>--%>
    </form>
    <footer>
        <p>© Copyright 2016, Yinon Manor & Arel Gindos</p>
    </footer>
</body>
    
</html>
