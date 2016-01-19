<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PdfTemplate.aspx.cs" Inherits="DotsolutionsWebsiteTester.PdfTemplate" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Testresultaten PDF</title>
    <style>
        html, body {
            /*height: 100%;*/
            font-size: 62.5%;
            font-family: 'Open Sans', sans-serif !important;
        }

        body {
            padding-bottom: 20px;
            background-color: #f5f5f5 !important;
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

        .panel-custom {
            padding: 25px;
        }

            .panel-custom .panel-heading span:not(.ratingCircle) {
                font-size: 1.3em;
            }

        .resultWell {
            width: 48%;
            float: left;
        }

        .resultBox {
            width: 48%;
            float: left;
            padding: 15px;
        }

        .resultBox-12 {
            width: 100%;
            float: left;
            padding: 15px;
        }

        .socialResults {
            width: 50%;
            float: left;
        }

        .socialResultBox-3 {
            padding: 20px 30px;
        }

        .socialResultBox-2 {
            padding: 40px 30px;
        }

            .socialResultBox-3 i, .socialResultBox-2 i {
                width: 15%;
                float: left;
            }

            .socialResultBox-3 span, .socialResultBox-2 span {
                width: 85%;
                float: left;
            }

        .resultDivider {
            width: 4%;
            float: left;
            min-height: 1px;
            position: relative;
        }

        .fa-html5 {
            color: #e54d26;
        }

        .fa-times, .fa-times-circle {
            color: red;
        }

        .fa-info-circle {
            color: lightblue;
        }

        .fa-check {
            color: green;
        }

        .fa-check-circle {
            color: green;
        }

        .fa-twitter-square {
            color: #55acee;
        }

        .fa-linkedin-square {
            color: #1884bc;
        }

        .fa-facebook-official, .fa-thumbs-o-up {
            color: #4862a3;
        }

        .fa-google-plus-square {
            color: #db4437;
        }

        .coverpicture {
            background-size: cover;
            background-position: center;
            /*height: 125px;;*/
            height: 250px;
            width: 50%;
            float: left;
        }

        .manualTest {
            left: 2%;
            position: relative;
        }

            .manualTest i {
                color: #f6cd29;
            }

            .manualTest#prof, .manualTest#userX {
                width: 50%;
                float: left;
            }

            .manualTest#vormComm {
                width: 100%;
                float: left;
            }

        #MainContent_manualResultHidden h3 {
            font-weight: bold;
        }


        MainContent_manualResultHidden #prof, MainContent_manualResultHidden #userX {
            float: left;
            width: 50%;
        }

        MainContent_manualResultHidden #vormComm {
            width: 100%;
            float: left;
        }

        #automatedRatingList {
            display: block;
        }

            #automatedRatingList ul {
                display: inline-block;
                width: 100%;
            }

        #critlistcontainer > div {
            width: 50%;
            float: left;
            min-height: 120px;
        }

        .ratingList {
            display: block;
            width: 80%;
            margin-left: 50px;
        }

            .ratingList li span {
                margin-left: -40px;
            }

            .ratingList li a {
                cursor: pointer;
                font-size: 1.3em;
            }

                .ratingList li a:hover, .ratingList li a.focus, .ratingList li a:focus {
                    text-decoration: none;
                }

        #testedsiteslist li {
            margin-bottom: 5px;
            list-style: none;
            margin-left: -25px;
            font-size: 1.1em;
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

        .score-0 {
            background-color: #FF0000;
        }

        .score-1 {
            background-color: #FF3200;
        }

        .score-2 {
            background-color: #FF6400;
        }

        .score-3 {
            background-color: #FF9600;
        }

        .score-4 {
            background-color: #FFC800;
        }

        .score-5 {
            background-color: #FFFF00;
        }

        .score-6 {
            background-color: #D4FF00;
        }

        .score-7 {
            background-color: #AAFF00;
        }

        .score-8 {
            background-color: #80FF00;
        }

        .score-9 {
            background-color: #55FF00;
        }

        .score-10 {
            background-color: #00FF00;
        }

        .infoScore {
            background-color: #892978;
        }

        .emptyScore {
            background-color: #0185AA;
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
            font-size: 1.2em;
        }

        .googleResult_url {
            color: #006621;
            font-size: 1em;
        }

        .googleResult_desc {
            color: #545454;
            font-size: 1em;
        }

        .largetext {
            font-size: 2em;
        }

        .emphasis {
            font-weight: 700;
            font-size: 1.5em;
        }

        tr:nth-child(2n+1) {
            background-color: #F5F5F5;
        }

        .tableImg {
            max-width: 50px;
            max-height: 50px;
        }

        .messageIcon {
            float: left;
            width: 5%;
        }

        .messageText {
            float: left;
            width: 95%;
        }

        #MainContent_sizeref {
            height: 380px;
        }

            #MainContent_sizeref h4 {
                font-size: 1.3em;
                font-weight: bold;
            }

        .laptopcontainer {
            width: 330px;
            height: 250px;
            background-image: url("http://i.imgur.com/AzbZXWr.png");
            padding: 15px 47px 100px 47px;
            display: block;
            background-size: contain;
            background-repeat: no-repeat;
            margin-right: auto;
            margin-left: auto;
            float: right;
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
            padding: 40px 19px 47px 10px;
            background-size: contain;
            background-repeat: no-repeat;
        }


        #MainContent_tabletImg {
            margin-left: 15%;
            margin-bottom: 10px;
        }

        #MainContent_mobileImg {
            margin-left: 0;
        }

        @media (min-width: 1024px) {
            #MainContent_tabletImg, #MainContent_mobileImg {
                margin-left: 25%;
            }
        }

        .title {
            font-weight: bold;
            font-size: 2.3em;
            margin-left: 10px;
        }

        .subTitle {
            font-size: 1.5em;
            margin-left: 10px;
        }

        @media all {
            .page-break {
                display: none;
            }
        }

        @media print {
            .page-break {
                display: block;
                page-break-before: always;
            }
        }
    </style>
    <link href="~/Content/images/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css" />
    <link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/font-awesome/4.5.0/css/font-awesome.min.css" />
    <link href='https://fonts.googleapis.com/css?family=Open+Sans' rel='stylesheet' type='text/css' />
    <meta charset="UTF-8" />
</head>
<body>
    <form runat="server">
        <div class="container body-content">
            <div id="sizeref" runat="server"></div>
            <div id="manualresults" runat="server"></div>
            <div class="panel panel-custom" id="automatedRatingList">
                <div class="panel-heading">
                    <span>Beoordeling geautomatiseerde test</span>
                </div>
                <div class="panel-body">
                    <div>
                        <span id="RatingOverall" class="mediocreScore ratingCircle" runat="server">...</span><span class="subTitle">Totaal</span>
                    </div>
                    <hr />
                    <div id="critlistcontainer" runat="server">
                        <div id="RatingAccessTxt" runat="server">
                            <span id="RatingAccess" class="ratingSquare" runat="server">...</span><span class="subTitle">Toegankelijkheid</span>
                            <div class="ratingList">
                                <ul id="RatingAccessList" runat="server">
                                </ul>
                            </div>
                        </div>
                        <div id="RatingUxTxt" runat="server">
                            <span id="RatingUx" class="ratingSquare" runat="server">...</span><span class="subTitle">Gebruikerservaring</span>
                            <div class="ratingList">
                                <ul id="RatingUxList" runat="server">
                                </ul>
                            </div>
                        </div>
                        <div id="RatingMarketingTxt" runat="server">
                            <span id="RatingMarketing" class="ratingSquare" runat="server">...</span><span class="subTitle">Marketing</span>
                            <div class="ratingList">
                                <ul id="RatingMarketingList" runat="server">
                                </ul>
                            </div>
                        </div>
                        <div id="RatingTechTxt" runat="server">
                            <span id="RatingTech" class="ratingSquare" runat="server">...</span><span class="subTitle">Technologie</span>
                            <div class="ratingList">
                                <ul id="RatingTechList" runat="server">
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div class="panel panel-custom">
                <div class="panel-heading">
                    <span>Deze pagina's van uw website hebben wij voor u getest</span>
                </div>
                <div class="panel-body">
                    <ul id="testedsiteslist" runat="server"></ul>
                </div>
            </div>

            <div id="results" runat="server">
            </div>

        </div>
    </form>
</body>
</html>

