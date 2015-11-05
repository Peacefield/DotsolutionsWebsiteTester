<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfTemplate.aspx.cs" Inherits="DotsolutionsWebsiteTester.PdfTemplate" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Testresultaten PDF</title>
    <style>
        /* Move down content because we have a fixed navbar that is 50px tall */
        body {
            padding-bottom: 20px;
            background-color: #396B02 !important;
            /*background: url("/Content/images/dotlogo.png") fixed no-repeat 100% 100%;*/
            -webkit-background-size: cover;
            -moz-background-size: cover;
            -o-background-size: cover;
            background-size: cover;
            height: 100%;
            font-size: 1em;
        }

        /* Wrapping element */
        /* Set some basic padding to keep content from hitting the edges */
        .body-content {
            padding-left: 15px;
            padding-right: 15px;
        }

        .results {
            overflow-x: auto;
        }

        .word-break {
            word-break: break-word;
        }

        /* Custom panel colouring */
        .panel-custom {
            border-color: #85CC76 !important;
        }

            .panel-custom .panel-heading {
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
                margin-left: -40px;
            }

            .ratingList li a {
                cursor: pointer;
            }


        .lowScore {
            /*background-image: linear-gradient(to bottom,red 0,black 175%);*/
            background-color: red;
        }

        .mediocreScore {
            /*background-image: linear-gradient(to bottom,orangered 0,black 175%);*/
            background-color: orangered;
        }

        .excellentScore {
            /*background-image: linear-gradient(to bottom,green 0,black 175%);*/
            background-color: green;
        }

        .ratingCircle {
            display: inline-block;
            font-size: 2em;
            color: white;
            border-radius: 200px;
            margin-right: 5px;
            padding: 5px;
            width: 50px;
            max-width: 60px;
            height: 50px;
            text-align: center;
        }

        .ratingSquare {
            width: 35px;
            height: 35px;
            padding: 2px 2px 2px 2px;
            text-align: center;
            display: inline-block;
            font-size: 1.5em;
            color: white;
            border-radius: 5px;
            margin-right: 5px;
            margin-bottom: 4px;
        }

        .googleResult_title, .googleResult_url, .googleResult_desc {
            margin: 0 0 0 0;
            cursor: default;
        }

        .googleResult_title {
            color: #1a0dab;
            text-decoration: underline;
            font-size: 1.3em;
        }

        .googleResult_url {
            color: #006621;
            font-size: 1em;
        }

        .googleResult_desc {
            color: #545454;
            font-size: 1em;
        }

        .tabletcontainer {
            width: 450px;
            height: 260px;
            background-image: url("http://i.imgur.com/BTpnDci.png");
            padding: 26px 141px 26px 31px;
            display: inline-block;
            background-size: contain;
            background-repeat: no-repeat;
        }

        .mobilecontainer {
            width: 144px;
            height: 260px;
            background-image: url("http://i.imgur.com/BDKytvF.png");
            padding: 40px 20px 47px 10px;
            background-size: contain;
            background-repeat: no-repeat;
        }

        #MainContent_tabletImg, #MainContent_mobileImg {
            margin-left: 25%;
        }
    </style>
    <link href="~/Content/images/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css" />
    <link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css" />
    <meta charset="UTF-8" />
</head>
<body>
    <form runat="server">
        <div class="container body-content">
            <div class="well well-sm" id="sizeref">
                <h5>Rapport voor</h5>
                <h4 id="UrlTesting" runat="server"></h4>
            </div>

            <div id="manualresults" runat="server"></div>
            <div class="well well-sm" id="automatedRatingList">
                <h3>Beoordeling Geautomatiseerde Test</h3>
                <div>
                    <span id="RatingOverall" class="mediocreScore ratingCircle" runat="server">...</span>Totaal
                </div>
                <hr />
                <ul runat="server">
                    <li id="RatingAccessTxt" runat="server"><span id="RatingAccess" class="ratingSquare" runat="server">...</span>Toegankelijkheid
                <div class="well-sm well ratingList">
                    <ul id="RatingAccessList" runat="server">
                    </ul>
                </div>
                    </li>
                    <li id="RatingUxTxt" runat="server"><span id="RatingUx" class="ratingSquare" runat="server">...</span>Gebruikerservaring
                <div class="well-sm well ratingList">
                    <ul id="RatingUxList" runat="server">
                    </ul>
                </div>
                    </li>
                    <li id="RatingMarketingTxt" runat="server"><span id="RatingMarketing" class="ratingSquare" runat="server">...</span>Marketing
                <div class="well-sm well ratingList">
                    <ul id="RatingMarketingList" runat="server">
                    </ul>
                </div>
                    </li>
                    <li id="RatingTechTxt" runat="server"><span id="RatingTech" class="ratingSquare" runat="server">...</span>Technologie
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

        </div>
    </form>
</body>
</html>

