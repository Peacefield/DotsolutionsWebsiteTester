<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="SocialInterest.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.SocialInterest" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="SocialInterest">
            <div class="panel panel-custom" id="SocialInterestSession" runat="server">
                <div class="panel-heading">
                    <span id="SocialInterestRating" runat="server" class="ratingCircle" style="background-color: #289BFE">?</span> Sociale interesse
                </div>
                <div class="panel-body">
                    <div id="SocialInterestResults" runat="server"></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
