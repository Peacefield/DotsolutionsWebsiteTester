function setup_loading() {
    // Set-up loading div
    $t = $(".body-content");

    $("#overlay").css({
        opacity: 0.8,
        top: $t.offset().top,
        width: $t.outerWidth(),
        height: $t.outerHeight()
    });

    $("#progressbar").css({
        width: $t.outerWidth(),
        top: ($t.height() / 2)
    });

    $("#loadingGIF").css({
        top: ($t.height() / 2) + $("#progressbar").height(),
        left: ($t.width() / 2) - ($(".loadingmid").width())
    });

    $("#TestProgress").css({

        top: ($t.height() / 2) + $("#progressbar").height() + $("#loadingGIF").height(),
        width: $t.outerWidth()
        //left: ($t.width() / 2.5)
    });
}

window.onresize = function () {
    setup_loading();
}

// Execute on window.onload
window.onload = function () {
    var url = $("#MainContent_UrlTesting").text();
    if (url != "") {

        var array = new Array();
        var finishedTests = 0;

        setup_loading();

        // Get everything ready for automatic testing
        $.ajax({
            url: "/TestTools/Start.aspx",
            cache: false,
            async: false,
            success: function (response) {
                // Do something
                $("#result").append($(response).find('#result').html());
                $("#overlay").css("height", $t.outerHeight());
            },
            error: function (response) {
                // Show error within results
                $("#result").append($(response).text);
            }
        });

        // Start automatic testing
        $("#MainContent_performedTests li").each(function (index) {
            var test = $(this).text().replace(" ", "");
            array.push(test);

            $("#TestsInProgress").append("<li>" + test + "</li>");
        });

        if (window.canRunAds == undefined) {
            var removeItem = "Analytics";
            array = jQuery.grep(array, function (value) {
                return value != removeItem;
            });
        }

        if (array.length > 0) {
            // Show loading div
            $("#overlay").fadeIn();
        }

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
                    $("#testprogress").css("width", progress + "%");
                    $("#progresstext").text(progress + "% compleet");
                    $("#overlay").css("height", $t.outerHeight());

                    $("#TestsInProgress li:contains(" + value + ")").remove();
                    $("#TestsComplete").append("<li>" + value + "</li>");

                    if (progress == 100) {
                        setTimeout(function () {
                            $("#overlay").fadeOut();
                            $("#progressbar").css("display", "none");
                            $("#MainContent_CreatePdfBtn").css("display", "block");
                            $("#performedTestshidden").css("display", "block");
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
};

// Hide Back to top button
$(document).ready(function () {
    $("#back-to-top").hide();
    $(window).scroll(function () {
        if ($(window).scrollTop() > 200) {
            $('#back-to-top').fadeIn();
        } else {
            $('#back-to-top').fadeOut();
        }
    });
    $('#back-to-top img').click(function () {
        $('body').animate({
            scrollTop: 0
        }, 1000);
    });
});