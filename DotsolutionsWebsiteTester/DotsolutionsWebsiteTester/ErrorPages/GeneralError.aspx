<%@ Page Title="Error" Language="C#" MasterPageFile="../Site.Master" AutoEventWireup="true" CodeBehind="GeneralError.aspx.cs" Inherits="DotsolutionsWebsiteTester.ErrorPages.GeneralError" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron text-center center-block">
        <h1>Er is iets misgegaan</h1>
        <pre>
            Error Message:<br />
            <asp:Label ID="exMessage" runat="server" 
                Font-Size="Larger" />
        </pre>
    </div>
    <script type="text/javascript" src="../Scripts/Custom/ads.js"></script>
    <script>
        if (window.canRunAds == undefined) {
            $("#adblocker").removeClass("hidden");
        }
    </script>

</asp:Content>
