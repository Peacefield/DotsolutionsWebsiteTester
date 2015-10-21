function CheckAll() {
    $("#MainContent_TestsCheckBoxList1 td input").each(function (index) {
        if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true);
        }
    });
    $("#MainContent_TestsCheckBoxList2 td input").each(function (index) {
        if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true);
        }
    });
    $("#MainContent_TestsCheckBoxList3 td input").each(function (index) {
        if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true);
        }
    });
}
function CheckNone() {
    $("#MainContent_TestsCheckBoxList1 td input").each(function (index) {
            $(this).prop("checked", false);
    });
    $("#MainContent_TestsCheckBoxList2 td input").each(function (index) {
        $(this).prop("checked", false);
    });
    $("#MainContent_TestsCheckBoxList3 td input").each(function (index) {
        $(this).prop("checked", false);
    });
}
window.onload = function () {
    var ShowCheckboxes = document.getElementById("ShowCheckboxes");
    var CheckAllCheckboxes = document.getElementById("CheckAllCheckboxes");
    var UrlTextBox = document.getElementById("MainContent_UrlTextBox");

    ShowCheckboxes.onclick = function () {
        setTimeout(function () {
            $("#checkboxHolder").toggle("slide", { direction: 'up' }, 1000, null);
        }, 100); // How long do you want the delay to be (in milliseconds)? 
        $(this).text(function (i, text) {
            return text === "Selecteer tests" ? "Verberg tests" : "Selecteer tests";
        })
    }

    //CheckAll();

    CheckAllCheckboxes.onclick = function () {
        if ($(this).text() === "Alles selecteren")
            CheckAll();
        else if ($(this).text() === "Niks selecteren")
            CheckNone();

        $(this).text(function (i, text) {
            return text === "Niks selecteren" ? "Alles selecteren" : "Niks selecteren";
        })
    }

    UrlTextBox.onkeyup = function () {
        if (UrlTextBox.value.indexOf("http") === -1 && UrlTextBox.value.indexOf("https") === -1) {
            //alert();
            var temp = "";
            if (UrlTextBox.value !== "") {
                temp = UrlTextBox.value;
            }
            UrlTextBox.value = "http://www." + temp;
        }
    }
}

$(document).ready(function () {
    $('#MainContent_UrlTextBox').focus(function () {
        this.selectionStart = this.selectionEnd = this.value.length;
    });
    $('#MainContent_UrlTextBox').select();
});