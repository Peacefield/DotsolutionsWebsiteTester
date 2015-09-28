<%@ Page Title="Aan het testen..." Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ProcessTest.aspx.cs" Inherits="DotsolutionsWebsiteTester.ProcessTest" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div id="overlay">
        <div id="progressbar" class="progress progress-striped active">
            <div id="testprogressbar" class="progress-bar" style="width: 0%">
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
            CssClass="btn btn-primary btn-md"
            OnClick="CreatePdfBtn_Click"
            runat="server" />
    </div>

    <div class="well well-sm" id="sizeref">
        <h5>Rapport voor: </h5>
        <h4 id="UrlTesting" runat="server"></h4>
    </div>

    <div id="performedTestshidden" class="well well-sm">
        <h3>Uitgevoerde tests:</h3>
        <ul id="performedTestsName" runat="server"></ul>
    </div>

    <div class="hidden">
        <ul id="performedTests" runat="server"></ul>
    </div>

    <div id="result"></div>
    
    <span id="back-to-top">
        <img src='Content/images/up-arrow.png' width="64" height="64"/>
    </span>
    <script type="text/javascript" src="Scripts/Custom/processTest.js"></script>
</asp:Content>