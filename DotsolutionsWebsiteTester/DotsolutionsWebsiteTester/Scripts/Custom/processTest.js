function setup_loading() {
    // Set-up loading div
    $bc = $(".body-content");
    $html = $("html");

    var right = parseInt($bc.css("padding-right"));
    var left = parseInt($bc.css("padding-left"));
    var outerWidth = parseInt($bc.outerWidth());
    var width = outerWidth - (right + left);

    $("#overlay").css({
        opacity: 0.8,
        top: $bc.offset().top,
        width: width,
        height: $html.outerHeight,
        right: "auto"
    });

    $("#progressbar").css({
        width: width,
        top: 200
    });

    $("#loadingGIF").css({
        top: 210 + $("#progressbar").height(),
        left: ($bc.width() / 2) - ($("#loadingGIF").width())
    });

    $("#testProgress").css({
        top: 200 + $("#progressbar").height() + $("#loadingGIF").height(),
        width: width,
        //left: ($bc.width() / 2.5)
    });
}

function animateTo(identifier) {
    event.srcElement.removeAttribute("href");
    var target = document.getElementById(identifier);
    $('body,html').animate({
        scrollTop: target.offsetTop - 50
    }, 1000);
}

function SetRatingClass(identifier, rating, overall) {
    if (rating === "-1")
        $(identifier).attr("class", "emptyScore ratingCircle");
    else {
        var rating = rating.replace(",", ".");
        if (overall) {
            //if (rating < 6)
            //    $(identifier).attr("class", "lowScore ratingCircle");
            //else if (rating < 8.5)
            //    $(identifier).attr("class", "mediocreScore ratingCircle");
            //else
            //    $(identifier).attr("class", "excellentScore ratingCircle");
            
            if (rating == 10)
                $(identifier).attr("class", "score-10 ratingCircle");
            else if (rating > 9)
                $(identifier).attr("class", "score-9 ratingCircle");
            else if (rating > 8)
                $(identifier).attr("class", "score-8 ratingCircle");
            else if (rating > 7)
                $(identifier).attr("class", "score-7 ratingCircle");
            else if (rating > 6)
                $(identifier).attr("class", "score-6 ratingCircle");
            else if (rating > 5)
                $(identifier).attr("class", "score-5 ratingCircle");
            else if (rating > 4)
                $(identifier).attr("class", "score-4 ratingCircle");
            else if (rating > 3)
                $(identifier).attr("class", "score-3 ratingCircle");
            else if (rating > 2)
                $(identifier).attr("class", "score-2 ratingCircle");
            else if (rating > 1)
                $(identifier).attr("class", "score-1 ratingCircle");
            else
                $(identifier).attr("class", "score-0 ratingCircle");


        }
        else {
            //if (rating < 6)
            //    $(identifier).attr("class", "lowScore ratingSquare");
            //else if (rating < 8.5)
            //    $(identifier).attr("class", "mediocreScore ratingSquare");
            //else
            //    $(identifier).attr("class", "excellentScore ratingSquare");

            if (rating == 10)
                $(identifier).attr("class", "score-10 ratingSquare");
            else if (rating > 9)
                $(identifier).attr("class", "score-9 ratingSquare");
            else if (rating > 8)
                $(identifier).attr("class", "score-8 ratingSquare");
            else if (rating > 7)
                $(identifier).attr("class", "score-7 ratingSquare");
            else if (rating > 6)
                $(identifier).attr("class", "score-6 ratingSquare");
            else if (rating > 5)
                $(identifier).attr("class", "score-5 ratingSquare");
            else if (rating > 4)
                $(identifier).attr("class", "score-4 ratingSquare");
            else if (rating > 3)
                $(identifier).attr("class", "score-3 ratingSquare");
            else if (rating > 2)
                $(identifier).attr("class", "score-2 ratingSquare");
            else if (rating > 1)
                $(identifier).attr("class", "score-1 ratingSquare");
            else
                $(identifier).attr("class", "score-0 ratingSquare");
        }
    }
}

var testedcriteria = 0;

function OnAccessSuccess(response) {
    if (response !== "-1") {
        $("#RatingAccess").text(response);
        testedcriteria++;
        SetRatingClass("#RatingAccess", response);
    }
}
function OnUserxSuccess(response) {
    if (response !== "-1") {
        $("#RatingUx").text(response);
        testedcriteria++;
        SetRatingClass("#RatingUx", response);
    }
}
function OnMarketingSuccess(response) {
    if (response !== "-1") {
        $("#RatingMarketing").text(response);
        testedcriteria++;
        SetRatingClass("#RatingMarketing", response);
    }
}
function OnTechSuccess(response) {
    if (response !== "-1") {
        $("#RatingTech").text(response);
        testedcriteria++;
        SetRatingClass("#RatingTech", response);
    }
}

function GetOverallRating() {
    var access = $("#RatingAccess").text();
    var userx = $("#RatingUx").text();
    var market = $("#RatingMarketing").text();
    var tech = $("#RatingTech").text();

    // Get acces rating
    if (access !== "-") {
        if ($("#RatingAccess").text().indexOf(",") > 0)
            access = $("#RatingAccess").text().replace(",", ".");
    }
    else
        access = 0.0;

    // Get user experience rating
    if (userx !== "-") {
        if ($("#RatingUx").text().indexOf(",") > 0)
            userx = $("#RatingUx").text().replace(",", ".");
    }
    else
        userx = 0.0;

    // Get marketing rating
    if (market !== "-") {
        if ($("#RatingMarketing").text().indexOf(",") > 0)
            market = $("#RatingMarketing").text().replace(",", ".");
    }
    else
        market = 0.0;

    // Get technology rating
    if (tech !== "-") {
        if ($("#RatingTech").text().indexOf(",") > 0)
            tech = $("#RatingTech").text().replace(",", ".");
    }
    else
        tech = 0.0;

    var total = ((parseFloat(access) + parseFloat(userx) + parseFloat(market) + parseFloat(tech)) / testedcriteria);

    if (isNaN(total)) {
        total = "-1";
    }
    else {
        if (total !== 10) {
            total = total.toFixed(1);
            $("#RatingOverall").text(total.replace(".", ","));
        }
        else {
            total = total.toFixed(0);
            $("#RatingOverall").text(total);
        }
    }

    PageMethods.AddOverallRatingSession(total, OnSuccess, OnError);

    SetRatingClass("#RatingOverall", total, true);
}

function OnAccesListSuccess(response) {
    $("#RatingAccessList").html(response);
}
function OnUxListSuccess(response) {
    $("#RatingUxList").html(response);
}
function OnMarketingListSuccess(response) {
    $("#RatingMarketingList").html(response);
}
function OnTechListSuccess(response) {
    $("#RatingTechList").html(response);
}

function OnSuccess(response) {
    // Do nothing
}
function OnError(error) {
    //alert(error);
    console.log(error);
}

window.onresize = function () {
    setup_loading();
}

// Execute on window.onload
window.onload = function () {
    PageMethods.set_path('Geautomatiseerde-Test.aspx')
    PageMethods.ResetRating(OnSuccess, OnError);
    var url = $("#MainContent_UrlTesting").text();
    if (url !== "") {

        var array = new Array();
        var finishedTests = 0;

        // Start automatic testing
        $("#MainContent_performedTests li").each(function (index) {
            var test = $(this).text().replace(" ", "");
            array.push(test);

            $("#testsInProgress").append("<li>" + test + "</li>");
        });

        // Remove Analytics from array since it will be blocked because of adblockers
        // Other tests will still be performed and Analytics will still be shown in the list of performed tests
        if (window.canRunAds === undefined) {
            var removeItem = "Analytics";
            array = jQuery.grep(array, function (value) {
                return value != removeItem;
            });
            $("#result").append("<div class = 'panel panel-danger' id='" + removeItem + "'>"
                + "<div class = 'panel-heading'>" + removeItem + "</div>"
                + "<div class = 'panel-body'>Test niet uitgevoerd, mogelijk in verband met adblocker</div></div>");
        }

        console.time("Execute tests");
        if (array.length !== 0) {
            $.each(array, function (index, value) {
                $.ajax({
                    url: "/TestTools/" + value + ".aspx",
                    cache: false,
                    async: true,
                    success: function (response) {
                        // Do something
                        finishedTests++;
                        var progress = ((finishedTests / array.length) * 100).toFixed(0);

                        $("#result").append($(response).find('#result').html());
                        $("#testprogressbar").css("width", progress + "%");
                        $("#progresstext").text(progress + "% compleet");
                        $("#overlay").css("height", $html.outerHeight());

                        $("#testsInProgress li:contains(" + value + ")").remove();
                        $("#testsComplete").append("<li>" + value + "</li>");

                        if (progress == 100) {
                            console.timeEnd("Execute tests");
                            $("#progresstext").text("Totaalscore's berekenen");

                            PageMethods.GetAccessRating(OnAccessSuccess, OnError);
                            PageMethods.GetUserxRating(OnUserxSuccess, OnError);
                            PageMethods.GetMarketingRating(OnMarketingSuccess, OnError);
                            PageMethods.GetTechRating(OnTechSuccess, OnError);

                            PageMethods.GetRatingAccessList(OnAccesListSuccess, OnError);
                            PageMethods.GetRatingUxList(OnUxListSuccess, OnError);
                            PageMethods.GetRatingMarketingList(OnMarketingListSuccess, OnError);
                            PageMethods.GetRatingTechList(OnTechListSuccess, OnError);

                            // Timeout because it has to be executed after WebMethods
                            setTimeout(function () {
                                GetOverallRating();
                                setTimeout(function () {
                                    // Set InnerHTML of Rating Lists in Session for PDF
                                    var accessInner = document.getElementById("RatingAccessList").innerHTML;
                                    var uxInner = document.getElementById("RatingUxList").innerHTML;
                                    var marketInner = document.getElementById("RatingMarketingList").innerHTML;
                                    var techInner = document.getElementById("RatingTechList").innerHTML;

                                    PageMethods.AddCriteriaListSession("RatingAccessList", accessInner, OnSuccess, OnError);
                                    PageMethods.AddCriteriaListSession("RatingUxList", uxInner, OnSuccess, OnError);
                                    PageMethods.AddCriteriaListSession("RatingMarketingList", marketInner, OnSuccess, OnError);
                                    PageMethods.AddCriteriaListSession("RatingTechList", techInner, OnSuccess, OnError);

                                    var accessClasses = document.getElementById("RatingAccess").className;
                                    var uXClasses = document.getElementById("RatingUx").className;
                                    var marketClasses = document.getElementById("RatingMarketing").className;
                                    var techClasses = document.getElementById("RatingTech").className;
                                    var overallClasses = document.getElementById("RatingOverall").className;

                                    PageMethods.AddCriteriaClassSession("RatingAccessClasses", accessClasses, OnSuccess, OnError);
                                    PageMethods.AddCriteriaClassSession("RatingUxClasses", uXClasses, OnSuccess, OnError);
                                    PageMethods.AddCriteriaClassSession("RatingMarketingClasses", marketClasses, OnSuccess, OnError);
                                    PageMethods.AddCriteriaClassSession("RatingTechClasses", techClasses, OnSuccess, OnError);
                                    PageMethods.AddCriteriaClassSession("RatingOverallClasses", overallClasses, OnSuccess, OnError);

                                }, 1000);
                            }, 1000);

                            setTimeout(function () {
                                $("#overlay").fadeOut();
                                $("#automatedRatingList").fadeIn();
                                $("#MainContent_CreatePdfBtn").css("display", "block");
                                document.title = 'Resultaten - Website tester';
                            }, 5000);
                        }
                    },
                    error: function (response) {
                        // Show error within results
                        $("#result").append($(response).text);
                    }
                });
            });
        }
        else {
            $("#overlay").fadeOut();
            document.title = 'Resultaten - Website tester';
        }
    }
};

// Hide Back to top button
$(document).ready(function () {
    $("#overlay").fadeIn();

    setup_loading();

    $("#back-to-top").hide();
    $(window).scroll(function () {
        if ($(window).scrollTop() > 200) {
            $('#back-to-top').fadeIn();
        } else {
            $('#back-to-top').fadeOut();
        }
    });
    $('#back-to-top').click(function () {
        $('body,html').animate({
            scrollTop: 0
        }, 1000);
    });

    // Event Handlers showing tests
    var ShowRatingAccess = document.getElementById("ShowRatingAccess");
    var ShowRatingUx = document.getElementById("ShowRatingUx");
    var ShowRatingMarketing = document.getElementById("ShowRatingMarketing");
    var ShowRatingTech = document.getElementById("ShowRatingTech");
    ShowRatingAccess.onclick = function () {
        setTimeout(function () {
            $("#RatingAccessListHidden").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingAccesBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };
    ShowRatingUx.onclick = function () {
        setTimeout(function () {
            $("#RatingUxListHidden").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingUxBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };

    ShowRatingMarketing.onclick = function () {
        setTimeout(function () {
            $("#RatingMarketingListHidden").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingMarketingBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };
    ShowRatingTech.onclick = function () {
        setTimeout(function () {
            $("#RatingTechListHidden").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingTechBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };
});