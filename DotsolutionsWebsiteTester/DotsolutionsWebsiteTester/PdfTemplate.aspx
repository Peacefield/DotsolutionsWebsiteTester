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

        #automatedRatingList {
            display: block;
        }

            #automatedRatingList li {
                list-style: none;
            }

        .ratingSquare {
            font-size: 1.5em;
            font-size: 1.5em;
            border-radius: 5px;
            display: inline-block;
            background-color: orangered;
            padding-left: 3px;
            padding-right: 3px;
            margin-bottom: 4px;
            margin-right: 5px;
        }

        .resultWell {
            width: 48%;
            float: left;
        }

        .resultDivider {
            width: 4%;
            float: left;
            min-height: 1px;
            position: relative;
        }

        .manualTest {
            left: 2%;
            position: relative;
        }

        .ratingList {
            display: block;
            width: 30%;
            margin-left: 50px;
        }

            .ratingList li span {
                margin-left: -80px;
            }

            .ratingList li a {
                cursor: pointer;
            }


        .lowScore {
            color: white;
            background-image: linear-gradient(to bottom,red 0,black 175%);
        }

        .mediocreScore {
            color: white;
            background-image: linear-gradient(to bottom,orangered 0,black 175%);
        }

        .excellentScore {
            color: white;
            background-image: linear-gradient(to bottom,green 0,black 175%);
        }

        .ratingCircle {
            display: inline-block;
            max-width: 60px;
            width: 50px;
            height: 50px;
            font-size: 2em;
            border-radius: 200px;
            margin-right: 5px;
            padding: 5px;
            text-align: center;
        }
    </style>
    <div class="well well-sm" id="sizeref">
        <h5>Rapport voor</h5>
        <h4 id="UrlTesting" runat="server"></h4>
    </div>

    <div id="manualresults" runat="server"></div>
    <div class="well well-sm" id="automatedRatingList">
        <h3>Beoordeling Geautomatiseerde Test</h3>
        <ul runat="server">
            <li id="RatingAccessTxt" runat="server"><span id="RatingAccess" class="ratingSquare" runat="server">[Aan het berekenen...]</span>Toegankelijkheid
                <div class="well-sm well ratingList">
                    <ul id="RatingAccessList" runat="server">
                    </ul>
                </div>
            </li>
            <li id="RatingUxTxt" runat="server"><span id="RatingUx" class="ratingSquare" runat="server">[Aan het berekenen...]</span>Gebruikerservaring
                <div class="well-sm well ratingList">
                    <ul id="RatingUxList" runat="server">
                    </ul>
                </div>
            </li>
            <li id="RatingMarketingTxt" runat="server"><span id="RatingMarketing" class="ratingSquare" runat="server">[Aan het berekenen...]</span>Marketing
                <div class="well-sm well ratingList">
                    <ul id="RatingMarketingList" runat="server">
                    </ul>
                </div>
            </li>
            <li id="RatingTechTxt" runat="server"><span id="RatingTech" class="ratingSquare" runat="server">[Aan het berekenen...]</span>Technologie
                <div class="well-sm well ratingList">
                    <ul id="RatingTechList" runat="server">
                    </ul>
                </div>
            </li>
        </ul>
    </div>
    <div class="well well-sm">
        <h3>Uitgevoerde tests</h3>
        <ul id="performedTests" runat="server"></ul>
    </div>
    <div class="well well-sm">
        <h3>Geteste pagina's</h3>
        <ul id="testedsiteslist" runat="server"></ul>
    </div>
    <div id="results" runat="server">
    </div>

</asp:Content>
