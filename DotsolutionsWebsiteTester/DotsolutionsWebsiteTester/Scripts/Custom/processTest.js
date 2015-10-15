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

    $("#TestProgress").css({
        top: 200 + $("#progressbar").height() + $("#loadingGIF").height(),
        width: width,
        //left: ($bc.width() / 2.5)
    });
}

function animateTo(identifier) {
    var target = document.getElementById(identifier);
    $('body').animate({
        scrollTop: target.offsetTop - 50
    }, 1000);
}

function OnAccessSuccess(response) {
    $("#RatingAccess").text(response);
    if (response > 0) {
        $("#RatingAccessTxt").css("display", "list-item");
    }
}
function OnUserxSuccess(response) {
    $("#RatingUx").text(response);
    if (response > 0) {
        $("#RatingUxTxt").css("display", "list-item");
    }
}
function OnMarketingSuccess(response) {
    $("#RatingMarketing").text(response);
    if (response > 0) {
        $("#RatingMarketingTxt").css("display", "list-item");
    }
}
function OnTechSuccess(response) {
    $("#RatingTech").text(response);
    if (response > 0) {
        $("#RatingTechTxt").css("display", "list-item");
    }
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

            $("#TestsInProgress").append("<li>" + test + "</li>");
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

                        $("#TestsInProgress li:contains(" + value + ")").remove();
                        $("#TestsComplete").append("<li>" + value + "</li>");

                        if (progress == 100) {
                            PageMethods.GetAccessRating(OnAccessSuccess, OnError);
                            PageMethods.GetUserxRating(OnUserxSuccess, OnError);
                            PageMethods.GetMarketingRating(OnMarketingSuccess, OnError);
                            PageMethods.GetTechRating(OnTechSuccess, OnError);
                            setTimeout(function () {
                                $("#overlay").fadeOut();
                                $("#performedTestshidden").fadeIn();
                                $("#RatingList").fadeIn();
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
        $('body').animate({
            scrollTop: 0
        }, 1000);
    });
});