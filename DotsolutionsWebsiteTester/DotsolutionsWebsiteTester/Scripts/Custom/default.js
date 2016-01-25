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
function ChangeFontWeightCheckBox(identifier) {
    var input = document.getElementById(identifier);
    if (input.checked == true) {
        $("label[for='" + input.id + "']").css("font-weight", "bold");
    }
    else {
        $("label[for='" + input.id + "']").css("font-weight", "normal");
    }
}
function Toggle(self, identifier) {
    var input = document.getElementById(identifier);
    if (self.checked == true) {
        document.getElementById(identifier).checked = false;
    }
    ChangeFontWeightCheckBox('MainContent_TestCheckBox');
    ChangeFontWeightCheckBox('MainContent_ThreePageReportCheckBox');
}
window.onload = function () {
    var ShowCheckboxes = document.getElementById("ShowCheckboxes");
    var CheckAllCheckboxes = document.getElementById("CheckAllCheckboxes");
    var UrlTextBox = document.getElementById("MainContent_UrlTextBox");

    ShowCheckboxes.onclick = function () {
        setTimeout(function () {
            $("#checkboxHolder").toggle("slide", { direction: 'up' }, 1000, null);
        }, 100); // Delay (in milliseconds)
        $(this).text(function (i, text) {
            //return text === "Verberg tests" ? "Selecteer tests" : "Verberg tests";
            return text === "Selecteer tests" ? "Verberg tests" : "Selecteer tests";
        })
    }

    //CheckAll();
    ChangeFontWeight('MainContent_TestsCheckBoxList1');
    ChangeFontWeight('MainContent_TestsCheckBoxList2');
    ChangeFontWeight('MainContent_TestsCheckBoxList3');

    ChangeFontWeightCheckBox('MainContent_TestCheckBox');
    ChangeFontWeightCheckBox('MainContent_ThreePageReportCheckBox');


    CheckAllCheckboxes.onclick = function () {
        if ($(this).text() === "Alles selecteren")
            CheckAll();
        else if ($(this).text() === "Alles deselecteren")
            CheckNone();

        $(this).text(function (i, text) {
            return text === "Alles deselecteren" ? "Alles selecteren" : "Alles deselecteren";
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
        ChangeFontWeightCheckBox('MainContent_TestCheckBox');
        Toggle(this, 'MainContent_ThreePageReportCheckBox');
    };
    document.getElementById('MainContent_ThreePageReportCheckBox').onchange = function () {
        ChangeFontWeightCheckBox('MainContent_ThreePageReportCheckBox');
        Toggle(this, 'MainContent_TestCheckBox');
    };
}

$(document).ready(function () {
    $('#MainContent_UrlTextBox').focus(function () {
        this.selectionStart = this.selectionEnd = this.value.length;
    });
    $('#MainContent_UrlTextBox').select();
});