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
    var target = document.getElementById(identifier);
    $('body,html').animate({
        scrollTop: target.offsetTop - 50
    }, 1000);
}

function SetRatingDisplay(rating, criteria) {
    var rating = parseInt(rating);
    if (rating < 4) {
        $("#" + criteria).css("background-color", "red");
        $("#" + criteria).css("color", "white");
    }
    else if (rating < 8) {
        $("#" + criteria).css("background-color", "orangered");
        $("#" + criteria).css("color", "white");
    }
    else {
        $("#" + criteria).css("background-color", "green");
        $("#" + criteria).css("color", "white");
    }
}

function OnAccessSuccess(response) {
    $("#RatingAccess").text(response);
    if (response >= 0) {
        $("#RatingAccessTxt").css("display", "list-item");
    }
    SetRatingDisplay(response, "RatingAccess");
}
function OnUserxSuccess(response) {
    $("#RatingUx").text(response);
    if (response >= 0) {
        $("#RatingUxTxt").css("display", "list-item");
    }
    SetRatingDisplay(response, "RatingUx");
}
function OnMarketingSuccess(response) {
    $("#RatingMarketing").text(response);
    if (response >= 0) {
        $("#RatingMarketingTxt").css("display", "list-item");
    }
    SetRatingDisplay(response, "RatingMarketing");
}
function OnTechSuccess(response) {
    $("#RatingTech").text(response);
    if (response >= 0) {
        $("#RatingTechTxt").css("display", "list-item");
    }
    SetRatingDisplay(response, "RatingTech");
}


function OnSuccess(response) {
    // Do nothing
}
function OnError(error) {
    alert(error);
}

window.onresize = function () {
    setup_loading();
}

// Execute on window.onload
window.onload = function () {
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
            alert(removeItem);
            $("#result").append("<div class = 'panel panel-danger' id='" + removeItem + "'>"
                + "<div class = 'panel-heading'>" + removeItem + "</div>"
                + "<div class = 'panel-body'>Test niet uitgevoerd, mogelijk in verband met adblocker</div></div>");
        }

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
                            PageMethods.GetAccessRating(OnAccessSuccess, OnError);
                            PageMethods.GetUserxRating(OnUserxSuccess, OnError);
                            PageMethods.GetMarketingRating(OnMarketingSuccess, OnError);
                            PageMethods.GetTechRating(OnTechSuccess, OnError);
                            setTimeout(function () {
                                $("#overlay").fadeOut();
                                $("#performedTestshidden").fadeIn();
                                $("#automatedRatingList").fadeIn();
                                $("#MainContent_CreatePdfBtn").css("display", "block");
                                document.title = 'Resultaten - Website tester';
                            }, 1000);
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
            $("#performedTestshidden").css("display", "block");
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
            $("#MainContent_RatingAccessList").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingAccesBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };
    ShowRatingUx.onclick = function () {
        setTimeout(function () {
            $("#MainContent_RatingUxList").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingUxBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };

    ShowRatingMarketing.onclick = function () {
        setTimeout(function () {
            $("#MainContent_RatingMarketingList").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingMarketingBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };
    ShowRatingTech.onclick = function () {
        setTimeout(function () {
            $("#MainContent_RatingTechList").toggle("slide", { direction: 'up' }, 500, null);
        }, 100);

        $("#RatingTechBtn").prop("class", function (i, curClass) {
            return curClass === "glyphicon glyphicon-chevron-down" ? "glyphicon glyphicon-chevron-up" : "glyphicon glyphicon-chevron-down";
        })
    };
});