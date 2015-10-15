<%@ Page Title="Testresults" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="PdfTemplate.aspx.cs" Inherits="DotsolutionsWebsiteTester.PdfTemplate" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        @media (max-width: 767px) {
            .table-responsive {
                overflow: auto;
            }
        }

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
            display: inline-block;
            max-width: 60px;
            width: 50px;
            height: 50px;
            font-size: 2em;
            background-color: white;
            color: black;
            border-radius: 200px;
            margin-right: 5px;
            padding: 5px;
            text-align: center;
        }

        #RatingList {
            display: block;
        }

            #RatingList li {
                display: none;
            }

        .rating {
            font-size: 1.5em;
        }

        .ResultWell {
            width: 48%;
            float: left;
        }

        .ResultDivider {
            width: 4%;
            float: left;
            min-height: 1px;
            position: relative;
        }
    </style>
    <div class="well well-sm" id="sizeref">
        <h5>Rapport voor: </h5>
        <h4 id="UrlTesting" runat="server"></h4>
    </div>
    <div class="well well-sm" id="RatingList">
        <h3>Beoordeling:</h3>
        <ul runat="server">
            <li id="RatingAccessTxt" runat="server">Toegankelijkheid: <span id="RatingAccess" class="rating" runat="server">[Aan het berekenen...]</span></li>
            <li id="RatingUxTxt" runat="server">Gebruikerservaring: <span id="RatingUx" class="rating" runat="server">[Aan het berekenen...]</span></li>
            <li id="RatingMarketingTxt" runat="server">Marketing: <span id="RatingMarketing" class="rating" runat="server">[Aan het berekenen...]</span></li>
            <li id="RatingTechTxt" runat="server">Technologie: <span id="RatingTech" class="rating" runat="server">[Aan het berekenen...]</span></li>
        </ul>
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
