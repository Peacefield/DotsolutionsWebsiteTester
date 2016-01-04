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
            background-color: #54b721 !important;
            background: url(data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAoAAAAKCAYAAACNMs+9AAAAR0lEQVQYV2MM2a74n4EA+PLkDwMjIYUgRTwyLPgVwhSBLMRpIrIinArRFWFViE0RhkJcilAU4lMEV0hIEVihx2zZ/6BwIgQAQjIwjcA3DQgAAAAASUVORK5CYII=) repeat;
            /*background: url(http://v8.dotcontent.nl/media/img/bg.jpg) center center no-repeat fixed;*/
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

        .well.well-sm {
            box-shadow: 0px 0px 100px 1px #000;
        }
        /* Custom panel colouring */

        .panel.panel-custom {
            box-shadow: 0px 0px 50px 10px #000;
        }

        .panel-custom {
            /*border-color: #32A389 !important;*/
            border-color: #489b1d !important;
        }

            .panel-custom .panel-heading {
                color: white;
                /*background-color: #20856E;*/
                /*border-color: #32A389;*/
                background-color: #54b721;
                border-color: #489b1d;
                background-repeat: repeat-x;
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

        #MainContent_manualResultHidden h3 {
            font-weight: bold;
        }

        #automatedRatingList {
            display: block;
        }

            #automatedRatingList ul {
                display: inline-block;
                width: 100%;
            }

            #automatedRatingList > #critlistcontainer > li {
                list-style: none;
                width: 50%;
                float: left;
                min-height: 120px;
                margin-bottom: 25px;
            }

                #automatedRatingList > #critlistcontainer > li > .ratingList > ul > li {
                    margin-top: 5px;
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

        .largetext {
            font-size: 2em;
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
            height: 200px;
        }


            #MainContent_sizeref h5 {
                font-size: 1.5em;
            }

            #MainContent_sizeref h4 {
                font-size: 1.5em;
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
            text-transform: uppercase;
            font-weight: bold;
            font-size: 2.3em;
            margin-left: 10px;
        }

        .subTitle {
            text-transform: uppercase;
            font-size: 1.5em;
            margin-left: 10px;
        }
    </style>
    <link href="~/Content/images/favicon.ico" rel="shortcut icon" type="image/x-icon" />
    <link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/bootstrap/3.3.5/css/bootstrap.min.css" />
    <link rel="stylesheet" href="http://maxcdn.bootstrapcdn.com/font-awesome/4.4.0/css/font-awesome.min.css" />
    <link href='https://fonts.googleapis.com/css?family=Open+Sans' rel='stylesheet' type='text/css' />
    <meta charset="UTF-8" />
</head>
<body>
    <form runat="server">
        <div class="container body-content">
            <div id="sizeref" runat="server"></div>
            <div id="manualresults" runat="server"></div>
            <div class="well well-sm" id="automatedRatingList">
                <h3>Beoordeling geautomatiseerde test</h3>
                <div>
                    <span id="RatingOverall" class="mediocreScore ratingCircle" runat="server">...</span><span class="subTitle">Totaal</span>
                </div>
                <hr />
                <ul id="critlistcontainer" runat="server">
                    <li id="RatingAccessTxt" runat="server"><span id="RatingAccess" class="ratingSquare" runat="server">...</span><span class="subTitle">Toegankelijkheid</span>
                        <div class="ratingList">
                            <ul id="RatingAccessList" runat="server">
                            </ul>
                        </div>
                    </li>
                    <li id="RatingUxTxt" runat="server"><span id="RatingUx" class="ratingSquare" runat="server">...</span><span class="subTitle">Gebruikerservaring</span>
                        <div class="ratingList">
                            <ul id="RatingUxList" runat="server">
                            </ul>
                        </div>
                    </li>
                    <li id="RatingMarketingTxt" runat="server"><span id="RatingMarketing" class="ratingSquare" runat="server">...</span><span class="subTitle">Marketing</span>
                        <div class="ratingList">
                            <ul id="RatingMarketingList" runat="server">
                            </ul>
                        </div>
                    </li>
                    <li id="RatingTechTxt" runat="server"><span id="RatingTech" class="ratingSquare" runat="server">...</span><span class="subTitle">Technologie</span>
                        <div class="ratingList">
                            <ul id="RatingTechList" runat="server">
                            </ul>
                        </div>
                    </li>
                </ul>
            </div>
            <div class="well well-sm">
                <h3>Deze pagina's van uw website hebben wij voor u getest</h3>
                <ul id="testedsiteslist" runat="server"></ul>
            </div>
            <div id="results" runat="server">
            </div>

        </div>
    </form>
</body>
</html>

