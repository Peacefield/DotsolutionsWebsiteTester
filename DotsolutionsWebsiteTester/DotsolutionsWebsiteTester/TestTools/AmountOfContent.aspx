<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AmountOfContent.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.AmountOfContent" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="AmountOfContent">
            <div class="panel panel-custom" id="AmountOfContentSession" runat="server">
                <div class="panel-heading">
                    <span id="Rating" runat="server"></span> Hoeveelheid Content
                </div>
                <div class="panel-body">
                </div>
            </div>
        </div>

    </div>
</asp:Content>

