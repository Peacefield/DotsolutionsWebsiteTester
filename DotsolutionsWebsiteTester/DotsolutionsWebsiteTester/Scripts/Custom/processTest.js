﻿function setup_loading() {
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
        top: ($bc.height() / 2)
    });

    $("#loadingGIF").css({
        top: ($bc.height() / 2) + $("#progressbar").height(),
        left: ($bc.width() / 2) - ($(".loadingmid").width())
    });

    $("#TestProgress").css({
        top: ($bc.height() / 2) + $("#progressbar").height() + $("#loadingGIF").height(),
        width: width,
        //left: ($bc.width() / 2.5)
    });
}

function animateTo(identifier) {
    var target = document.getElementById(identifier);
    $('body').animate({
        scrollTop: target.offsetTop
    }, 1000);
}

window.onresize = function () {
    setup_loading();
}

// Execute on window.onload
window.onload = function () {

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
                            setTimeout(function () {
                                $("#overlay").fadeOut();
                                $("#performedTestshidden").css("display", "block");
                                $("#MainContent_CreatePdfBtn").css("display", "block");
                                document.title = 'Resultaten - Website tester';
                            }, 500);
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