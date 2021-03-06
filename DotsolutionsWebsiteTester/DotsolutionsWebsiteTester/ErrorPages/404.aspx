﻿<%@ Page Title="Pagina niet gevonden" Language="C#" MasterPageFile="../Site.Master" AutoEventWireup="true" CodeBehind="404.aspx.cs" Inherits="DotsolutionsWebsiteTester.ErrorPages._404" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron text-center center-block">
        <h1>Pagina niet gevonden</h1>

        <p>De opgevraagde pagina kan niet worden gevonden omdat deze is verplaatst of niet (meer) bestaat.</p>
        <asp:LinkButton ID="HistoryBackBtn"
            CssClass="btn btn-primary btn-md"
            runat="server"
            OnClientClick="history.go(-1)">
        <span aria-hidden="true" class="glyphicon glyphicon-circle-arrow-left"></span>
            Ga terug naar de vorige pagina
        </asp:LinkButton>

        <asp:LinkButton ID="HomeBackBtn"
            CssClass="btn btn-primary btn-md"
            runat="server"
            OnClick="HomeBackBtn_Click">
        <span aria-hidden="true" class="glyphicon glyphicon-home"></span>
            Ga terug naar de startpagina
        </asp:LinkButton>
    </div>
    <script type="text/javascript" src="../Scripts/Custom/ads.js"></script>
    <script>
        if (window.canRunAds == undefined) {
            $("#adblocker").removeClass("hidden");
        }
    </script>

</asp:Content>

