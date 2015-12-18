<%@ Page Title="Aan het testen..." Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Geautomatiseerde-Test.aspx.cs" Inherits="DotsolutionsWebsiteTester.ProcessTest" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div id="overlay">
        <div id="progressbar" class="progress progress-striped">
            <div id="testprogressbar" class="progress-bar progress-bar-success" style="width: 0%">
                <span id="progresstext">0% Compleet</span>
            </div>
        </div>
        <div>
            <img id="loadingGIF" src="Content/images/loading.gif" height="32" width="32" />
        </div>
        <div id="testProgress" class="row">
            <div class="col-md-6 col-lg-6 col-xs-6 col-sm-6 text-right">
                <h3>Testen in behandeling</h3>
                <ul id="testsInProgress"></ul>
            </div>
            <div class="col-md-6 col-lg-6 col-xs-6 col-sm-6">
                <h3>Testen compleet</h3>
                <ul id="testsComplete"></ul>
            </div>
        </div>
    </div>

    <div class="pull-right">
        <asp:Button ID="CreatePdfBtn"
            Text="Opslaan als PDF"
            CssClass="btn btn-success btn-md"
            OnClick="CreatePdfBtn_Click"
            runat="server" />
    </div>

    <div class="well well-sm" id="sizeref" runat="server">
        <asp:Image  runat="server" 
            ID="laptopcontainer" 
            CssClass="laptopcontainer" />
        <h5>Rapport voor</h5>
        <h4 id="UrlTesting" runat="server"></h4>
    </div>
    
    <div id="manualResultHidden" class="hidden" runat="server">
        <div class="well well-sm">
            <h3>Beoordeling handmatige test</h3>
            <hr />
            <h4>Vormgeving</h4>
            <div id="prof" class="manualTest">
                <h4>Professionaliteit</h4>
                <div class="row">
                    <label class="control-label col-md-2 col-lg-2 col-xs-4 col-sm-2">Opmaak</label>
                    <label class="col-md-9 col-lg-9 col-xs-7 col-sm-9" id="VormProfOpma" runat="server">
                    </label>
                </div>
                
                <div class="row">
                    <label class="control-label col-md-2 col-lg-2 col-xs-4 col-sm-2">Huisstijl</label>
                    <label class="col-md-9 col-lg-9 col-xs-7 col-sm-9" id="VormProfHuis" runat="server">
                    </label>
                </div>
                
                <div class="row">
                    <label class="control-label col-md-2 col-lg-2 col-xs-4 col-sm-2">Kleurgebruik</label>
                    <label class="col-md-9 col-lg-9 col-xs-7 col-sm-9" id="VormProfKleur" runat="server">
                    </label>
                </div>
            </div>
            <div id="userX" class="manualTest">
                <h4>Gebruiksvriendelijkheid</h4>
                <div class="row">
                    <label class="control-label col-md-2 col-lg-2 col-xs-4 col-sm-2">Menu</label>
                    <label class="col-md-9 col-lg-9 col-xs-7 col-sm-9" id="VormUxMen" runat="server">
                    </label>
                </div>
                
                <div class="row">
                    <label class="control-label col-md-2 col-lg-2 col-xs-4 col-sm-2">Structuur</label>
                    <label class="col-md-9 col-lg-9 col-xs-7 col-sm-9" id="VormUxStruc" runat="server">
                    </label>
                </div>
            </div>
            <div id="vormComm" class="manualTest">
                <div class="row">
                    <label class="control-label col-md-2 col-lg-2 col-xs-4 col-sm-2">Opmerkingen</label>
                    <label class="col-md-9 col-lg-9 col-xs-7 col-sm-9" id="VormComment" runat="server">
                    </label>
                    <label class="col-md-1 col-lg-1 col-xs-1 col-sm-1"></label>
                </div>
            </div>
        </div>
    </div>

    <div class="well well-sm" id="automatedRatingList">
        <h3>Beoordeling geautomatiseerde test</h3>
        <div>
            <span id="RatingOverall" class="mediocreScore ratingCircle">-</span>Totaal
        </div>
        <hr />
        <ul runat="server">
            <li id="RatingAccessTxt" runat="server"><span id="RatingAccess" class="ratingSquare emptyScore">-</span>Toegankelijkheid 
                <span id="ShowRatingAccess" class="displayRating">
                    <i id="RatingAccesBtn" class="glyphicon glyphicon-chevron-down"></i>
                </span> 
                <div id="RatingAccessListHidden" class="well-sm well ratingList">
                    <ul id="RatingAccessList">
                    </ul>
                </div>
            </li>
            <li id="RatingUxTxt"><span id="RatingUx" class="ratingSquare emptyScore">-</span>Gebruikerservaring
                <span id="ShowRatingUx" class="displayRating">
                    <i id="RatingUxBtn" class="glyphicon glyphicon-chevron-down"></i>
                </span> 
                <div id="RatingUxListHidden" class="well-sm well ratingList">
                    <ul id="RatingUxList">
                    </ul>
                </div>
            </li>
            <li id="RatingMarketingTxt"><span id="RatingMarketing" class="ratingSquare emptyScore">-</span>Marketing
                <span id="ShowRatingMarketing" class="displayRating">
                    <i id="RatingMarketingBtn" class="glyphicon glyphicon-chevron-down"></i>
                </span> 
                <div id="RatingMarketingListHidden" class="well-sm well ratingList">
                    <ul id="RatingMarketingList">
                    </ul>
                </div>
            </li>
            <li id="RatingTechTxt"><span id="RatingTech" class="ratingSquare emptyScore">-</span>Technologie
                <span id="ShowRatingTech" class="displayRating">
                    <i id="RatingTechBtn" class="glyphicon glyphicon-chevron-down"></i>
                </span> 
                <div id="RatingTechListHidden" class="well-sm well ratingList">
                    <ul id="RatingTechList">
                    </ul>
                </div>
            </li>
        </ul>
    </div>
    <!-- Hidden list with page-names used for the AJAX requests -->
    <div class="hidden">
        <ul id="performedTests" runat="server"></ul>
    </div>

    <div class="well well-sm">
        <h3>Geteste pagina's</h3>
        <ul id="TestedSitesList" runat="server"></ul>
    </div>

    <div id="result"></div>

    <div id="back-to-top">
        <div class="btn btn-success btn-md" id="ToTopBtn">
            <span aria-hidden="true" class="glyphicon glyphicon-arrow-up"></span>
            Terug naar boven
        </div>
    </div>

    <script type="text/javascript" src="Scripts/Custom/processTest.js"></script>
</asp:Content>
