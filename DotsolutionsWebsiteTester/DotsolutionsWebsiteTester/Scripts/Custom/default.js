function CheckAll() {
    $("#MainContent_TestsCheckBoxList1 td input").each(function (index) {
        if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true);
            $("label[for='" + $(this).prop("id") + "']").css("font-weight", "bold");
        }
    });
    $("#MainContent_TestsCheckBoxList2 td input").each(function (index) {
        if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true);
            $("label[for='" + $(this).prop("id") + "']").css("font-weight", "bold");
        }
    });
    $("#MainContent_TestsCheckBoxList3 td input").each(function (index) {
        if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true);
            $("label[for='" + $(this).prop("id") + "']").css("font-weight", "bold");
        }
    });
}
function CheckNone() {
    $("#MainContent_TestsCheckBoxList1 td input").each(function (index) {
        $(this).prop("checked", false);
        $("label[for='" + $(this).prop("id") + "']").css("font-weight", "normal");
    });
    $("#MainContent_TestsCheckBoxList2 td input").each(function (index) {
        $(this).prop("checked", false);
        $("label[for='" + $(this).prop("id") + "']").css("font-weight", "normal");
    });
    $("#MainContent_TestsCheckBoxList3 td input").each(function (index) {
        $(this).prop("checked", false);
        $(this).css("font-weight", "normal");
        $("label[for='" + $(this).prop("id") + "']").css("font-weight", "normal");
    });
}
function ChangeFontWeight(identifier) {
    var input = document.getElementById(identifier).getElementsByTagName("input")
    for (var i = 0; i < input.length; i++) {
        if (input[i].type == "checkbox") {
            if (input[i].checked == true) {
                $("label[for='" + input[i].id + "']").css("font-weight", "bold");
            }
            else {
                $("label[for='" + input[i].id + "']").css("font-weight", "normal");
            }
        }
    }
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
            // return text === "Verberg tests" ? "Selecteer tests" : "Verberg tests";
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
    document.getElementById('MainContent_TestsCheckBoxList1').onchange = function () { ChangeFontWeight('MainContent_TestsCheckBoxList1'); };
    document.getElementById('MainContent_TestsCheckBoxList2').onchange = function () { ChangeFontWeight('MainContent_TestsCheckBoxList2'); };
    document.getElementById('MainContent_TestsCheckBoxList3').onchange = function () { ChangeFontWeight('MainContent_TestsCheckBoxList3'); };
    document.getElementById('MainContent_TestCheckBox').onchange = function () {
        var input = document.getElementById('MainContent_TestCheckBox');
        if (input.checked == true) {
            $("label[for='" + input.id + "']").css("font-weight", "bold");
        }
        else {
            $("label[for='" + input.id + "']").css("font-weight", "normal");
        }
    };

    
}

$(document).ready(function () {
    $('#MainContent_UrlTextBox').focus(function () {
        this.selectionStart = this.selectionEnd = this.value.length;
    });
    $('#MainContent_UrlTextBox').select();
});