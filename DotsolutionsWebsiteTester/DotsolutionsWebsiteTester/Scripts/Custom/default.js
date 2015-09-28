window.onload = function () {
    var ShowCheckboxes = document.getElementById("ShowCheckboxes")
    var UrlTextBox = document.getElementById("MainContent_UrlTextBox");

    ShowCheckboxes.onclick = function () {
        setTimeout(function () {
            $("#CheckboxHolder").toggle("slide", { direction: 'up' }, 1000, null);
        }, 100); // How long do you want the delay to be (in milliseconds)? 
        $(this).text(function (i, text) {
            return text === "Selecteer tests" ? "Verberg tests" : "Selecteer tests";
        })
    }

    UrlTextBox.onkeyup = function () {
        if (UrlTextBox.value.indexOf("http") == -1 && UrlTextBox.value.indexOf("https") == -1) {
            //alert();
            var temp = "";
            if (UrlTextBox.value != "") {
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