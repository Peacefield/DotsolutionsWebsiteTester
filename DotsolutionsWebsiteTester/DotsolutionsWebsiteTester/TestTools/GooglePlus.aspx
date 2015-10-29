<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GooglePlus.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.GooglePlus" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="GooglePlus">
            <div class="panel panel-custom" id="GooglePlusSession" runat="server">
                <div class="panel-heading">
                    <span id="GooglePlusRating" runat="server">?</span> Google+
                </div>
                <div class="panel-body">
                <div id="GooglePlusResults" runat="server"></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>