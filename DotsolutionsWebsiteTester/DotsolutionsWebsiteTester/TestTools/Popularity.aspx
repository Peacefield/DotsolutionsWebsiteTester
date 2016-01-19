<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Popularity.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Popularity" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Popularity">
            <div class="panel panel-custom" id="PopularitySession" runat="server">
                <div class="panel-heading">
                    <span id="PopularityRating" runat="server">?</span><span class="title">Populariteit</span>
                </div>
                <div class="panel-body">
                    <div id="PopularityResults" class="results" runat="server"></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>