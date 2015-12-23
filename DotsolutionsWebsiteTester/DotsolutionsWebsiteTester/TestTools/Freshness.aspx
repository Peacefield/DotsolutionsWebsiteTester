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
                    <div class="hidden" id="FreshnessTableHidden" runat="server">
                        <asp:Table ID="FreshnessTable" CssClass="table table-hover word-break" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Datum" CssClass="col-md-2" />
                                <asp:TableHeaderCell Scope="Column" Text="Bestand" CssClass="col-md-5" />
                                <asp:TableHeaderCell Scope="Column" Text="Gevonden op" CssClass="col-md-5" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>