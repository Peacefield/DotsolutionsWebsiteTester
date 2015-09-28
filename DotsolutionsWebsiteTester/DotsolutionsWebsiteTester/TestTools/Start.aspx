<%@ Page Title="Start" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Start.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Start" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">
        <div class="well well-sm">
            <h3>Geteste sites:</h3>
            <ul id="testedsiteslist" runat="server"></ul>
        </div>
    </div>
</asp:Content>
