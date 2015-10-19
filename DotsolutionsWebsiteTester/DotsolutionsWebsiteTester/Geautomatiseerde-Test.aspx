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
        <div id="TestProgress" class="row">
            <div class="col-md-6 col-lg-6 col-xs-6 col-sm-6 text-right">
                <h3>Testen in behandeling</h3>
                <ul id="TestsInProgress"></ul>
            </div>
            <div class="col-md-6 col-lg-6 col-xs-6 col-sm-6">
                <h3>Testen compleet</h3>
                <ul id="TestsComplete"></ul>
            </div>
        </div>
    </div>

    <div class="pull-right">
        <asp:Button ID="CreatePdfBtn"
            Text="To PDF"
            CssClass="btn btn-success btn-md"
            OnClick="CreatePdfBtn_Click"
            runat="server" />
    </div>

    <div class="well well-sm" id="sizeref">
        <h5>Rapport voor: </h5>
        <h4 id="UrlTesting" runat="server"></h4>
    </div>

    <div class="well well-sm" id="RatingList">
        <h3>Beoordeling:</h3>
        <ul runat="server">
            <li id="RatingAccessTxt"><span id="RatingAccess" class="rating">[Aan het berekenen...]</span>Toegankelijkheid</li>
            <li id="RatingUxTxt"><span id="RatingUx" class="rating">[Aan het berekenen...]</span>Gebruikerservaring</li>
            <li id="RatingMarketingTxt"><span id="RatingMarketing" class="rating">[Aan het berekenen...]</span>Marketing</li>
            <li id="RatingTechTxt"><span id="RatingTech" class="rating">[Aan het berekenen...]</span>Technologie</li>
        </ul>
    </div>
    <div id="performedTestshidden" class="well well-sm">
        <h3>Uitgevoerde tests:</h3>
        <ul id="PerformedTestsName" runat="server"></ul>
    </div>
    <div class="well well-sm">
        <h3>Geteste sites:</h3>
        <ul id="TestedSitesList" runat="server"></ul>
    </div>

    <div class="hidden">
        <ul id="performedTests" runat="server"></ul>
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
