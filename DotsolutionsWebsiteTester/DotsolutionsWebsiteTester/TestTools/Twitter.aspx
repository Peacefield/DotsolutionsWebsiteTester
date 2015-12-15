<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Twitter.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Twitter" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Twitter">
            <div class="panel panel-custom" id="TwitterSession" runat="server">
                <div class="panel-heading">
                    <span id="TwitterRating" runat="server">?</span> Twitter
                </div>
                <div class="panel-body">
                <div id="twitterResults" runat="server"></div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>