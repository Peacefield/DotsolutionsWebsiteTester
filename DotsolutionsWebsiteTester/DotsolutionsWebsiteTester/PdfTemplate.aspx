<%@ Page Title="Testresults" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PdfTemplate.aspx.cs" Inherits="DotsolutionsWebsiteTester.PdfTemplate" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        /*@media (max-width: 767px) {
            .table-responsive {
                overflow: auto;
            }
        }*/

        @media (min-width: 768px) {
            .table-responsive {
                overflow: visible !important;
            }
        }
        /* Move down content because we have a fixed navbar that is 50px tall */
        body {
            padding-top: 50px;
            padding-bottom: 20px;
            background-image: none;
            background-size: cover;
            background-repeat: no-repeat;
        }

        /* Wrapping element */
        /* Set some basic padding to keep content from hitting the edges */
        .body-content {
            padding-left: 15px;
            padding-right: 15px;
        }
        /* Custom panel colouring */
        .panel-custom {
            border-color: #85CC76 !important;
        }

            .panel-custom .panel-heading {
                background-image: linear-gradient(to bottom,#85CC76 0,#145007 100%);
                color: white;
                background-color: #145007;
                background-repeat: repeat-x;
                border-color: #85CC76;
            }

        #MainContent_Rating {
            padding: 10px;
            background-color: white;
            color: black;
            border-radius: 200px;
            font-size: 2em;
        }
    </style>
    <div class="well well-sm" id="sizeref">
        <h5>Rapport voor: </h5>
        <h4 id="UrlTesting" runat="server"></h4>
    </div>
    <div class="well well-sm">
        <h3>Uitgevoerde tests:</h3>
        <ul id="performedTests" runat="server"></ul>
    </div>
    <div class="well well-sm">
        <h3>Geteste sites:</h3>
        <ul id="testedsiteslist" runat="server"></ul>
    </div>
    <div id="results" runat="server">
    </div>

</asp:Content>
