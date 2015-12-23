<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Freshness.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Freshness" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Freshness">
            <div class="panel panel-custom" id="FreshnessSession" runat="server">
                <div class="panel-heading">
                    <span id="FreshnessRating" runat="server">?</span><span class="title">Actueelheid</span>
                </div>
                <div class="panel-body">
                    <div id="FreshnessResults" class="results" runat="server"></div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>