function setup_loading() {
    // Set-up loading div
    $t = $(".body-content");
    
    var right = parseInt($t.css("padding-right"));
    var left = parseInt($t.css("padding-left"));
    var outerWidth = parseInt($t.outerWidth());
    var width = outerWidth - (right + left);

    $("#overlay").css({
        opacity: 0.7,
        top: $t.offset().top,
        width: width,
        height: $t.outerHeight(),
        right: "auto"
    });

    $("#progressbar").css({
        width: width,
        top: ($t.height() / 2)
    });

    $("#loadingGIF").css({
        top: ($t.height() / 2) + $("#progressbar").height(),
        left: ($t.width() / 2) - ($(".loadingmid").width())
    });

    $("#TestProgress").css({
        top: ($t.height() / 2) + $("#progressbar").height() + $("#loadingGIF").height(),
        width: width,
        //left: ($t.width() / 2.5)
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
    if (url != "") {

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
        if (window.canRunAds == undefined) {
            var removeItem = "Analytics";
            array = jQuery.grep(array, function (value) {
                return value != removeItem;
            });
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
                    $("#testprogressbar").css("width", progress + "%");
                    $("#progresstext").text(progress + "% compleet");
                    $("#overlay").css("height", $t.outerHeight());

                    $("#TestsInProgress li:contains(" + value + ")").remove();
                    $("#TestsComplete").append("<li>" + value + "</li>");

                    if (progress == 100) {
                        setTimeout(function () {
                            $("#overlay").fadeOut();
                            $("#performedTestshidden").css("display", "block");
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
    $('#back-to-top img').click(function () {
        $('body').animate({
            scrollTop: 0
        }, 1000);
    });
});