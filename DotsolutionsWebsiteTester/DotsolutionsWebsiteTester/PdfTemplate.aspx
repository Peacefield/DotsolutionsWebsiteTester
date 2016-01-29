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

        footer {
            height: 60px;
            padding: 18px 0px;
        }

            footer span {
                margin-right: 10px;
            }

            footer img {
                height: 20px;
                margin-bottom: 4px;
                margin-left: 3px;
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
                border-bottom: 1px solid #BEBDBD;
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
            color: #54b721;
        }

        .fa-check-circle {
            color: #54b721;
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

        #sizeref {
            width: 40%;
            float: left;
        }

        #CriteriaSummaryContainer {
            width: 60%;
            float: left;
        }

        #automatedRatingList, #testedpages, #results {
            width: 100%;
            float: left;
        }

            #automatedRatingList .panel-body {
                padding-top: 0;
            }

            #automatedRatingList .panel-heading {
                padding-bottom: 0;
            }

        #automatedRatingList, #CriteriaSummaryContainer {
            display: block;
        }

            #automatedRatingList ul, #CriteriaSummaryContainer ul {
                display: inline-block;
                width: 100%;
                list-style: none;
            }

            #automatedRatingList .ratingCircle {
                font-size: 1.5em;
                width: 40px;
                max-width: 50px;
                height: 40px;
            }

        #critlistcontainer {
            border-top: 1px solid #F5F5F5;
            padding-top: 5px;
            margin-top: 5px;
            float: left;
            width: 100%;
            display: block;
            font-size: .7em;
        }

            #critlistcontainer > div:not(#critlistpagebreak) {
                width: 50%;
                float: left;
                min-height: 120px;
            }

            #critlistcontainer .ratingSquare {
                width: 30px;
                height: 30px;
                padding: 5px 2px 2px 2px;
            }

        #critlistpagebreak {
            width: 100%;
            float: left;
            min-height: 200px;
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
                margin-left: 5px;
            }

                .ratingList li a:hover, .ratingList li a.focus, .ratingList li a:focus {
                    text-decoration: none;
                }

        #testedpages .panel-body {
            padding: 0;
        }

        #testedsiteslist li {
            margin-bottom: 5px;
            list-style: none;
            margin-left: -25px;
            font-size: 1em;
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

        .pieContainer {
            height: 100px;
            margin-left: 35%;
        }

        .pieBackground {
            position: absolute;
            width: 100px;
            height: 100px;
            -moz-border-radius: 50px;
            -webkit-border-radius: 50px;
            -o-border-radius: 50px;
            border-radius: 50px;
        }

        .pieInside {
            background-color: #f5f5f5;
            position: absolute;
            width: 80px;
            height: 80px;
            -moz-border-radius: 50px;
            -webkit-border-radius: 50px;
            -o-border-radius: 50px;
            border-radius: 50px;
            margin-top: 10px;
            margin-left: 10px;
            z-index: 1;
            color: #000;
        }

            .pieInside span {
                position: absolute;
                top: 22px;
                font-size: 1.8em;
            }

        .pie {
            position: absolute;
            -moz-border-radius: 50px;
            -webkit-border-radius: 50px;
            -o-border-radius: 50px;
            border-radius: 50px;
            clip: rect(0px, 50px, 100px, 0px);
            width: 100px;
            height: 100px;
        }

        .hold {
            position: absolute;
            width: 100px;
            height: 100px;
            -moz-border-radius: 50px;
            -webkit-border-radius: 50px;
            -o-border-radius: 50px;
            border-radius: 50px;
            clip: rect(0px, 100px, 100px, 50px);
        }


        .percentageContainer > div:first-child, .percentageContainer > div:nth-child(3) {
            height: 200px;
        }

            .percentageContainer > div:nth-child(3):not(.thirdPercentageChild) {
                padding-top: 50px;
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
            width: 250px;
            height: 250px;
            background-image: url("http://i.imgur.com/AzbZXWr.png");
            padding: 10px 36px 137px 35px;
            display: block;
            background-size: contain;
            background-repeat: no-repeat;
            margin-right: auto;
            margin-left: auto;
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
            height: 200px;
            background-image: url("http://i.imgur.com/BDKytvF.png");
            padding: 31px 48px 36px 7px;
            background-size: contain;
            background-repeat: no-repeat;
        }

        #MainContent_mobileImg {
            position: absolute;
            left: 45%;
            top: 100px;
        }

        #screenshotcontainer {
            height: 280px;
        }

            #screenshotcontainer > div {
                position: absolute;
                left: 33%;
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

        .page-break {
            display: block;
            page-break-before: always;
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

            <div id="CriteriaSummaryContainer" runat="server"></div>

            <div id="manualresults" runat="server"></div>

            <div class="page-break"></div>
            <div class="panel panel-custom" id="automatedRatingList">
                <div class="panel-heading">
                    <span>Beoordeling geautomatiseerde test</span>
                </div>
                <div class="panel-body">
                    <div id="RatingOverallHidden" class="hidden" runat="server">
                        <span id="RatingOverall" class="mediocreScore ratingCircle" runat="server">...</span><span class="subTitle">Totaal</span>
                    </div>
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
                        <div id="critlistpagebreak" class="page-break" runat="server"></div>
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

            <div class="page-break"></div>

            <div class="panel panel-custom" id="testedpages">
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


        <footer>
            <div class="col-md-12 text-center">
                <span class="text-muted">Ontwikkeld door</span>
                <a href="http://www.dotsolutions.nl" target="_blank">
                    <img src="http://www.dotsolutions.nl/images/logo.svg" alt="dotsolutions logo" />
                </a>
            </div>
        </footer>
    </form>
</body>
</html>

