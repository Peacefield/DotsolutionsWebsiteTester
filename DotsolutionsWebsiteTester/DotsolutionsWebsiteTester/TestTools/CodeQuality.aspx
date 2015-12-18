<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CodeQuality.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.CodeQuality" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="CodeQuality">
            <div class="panel panel-custom" id="CodeQualitySession" runat="server">
                <div class="panel-heading">
                    <span id="CodeQualityRating" runat="server">?</span><span class="title">Code kwaliteit</span>
                </div>
                <div class="panel-body">
                    <div id="w3ErrorsFound" class="results" runat="server"></div>
                    <div class="hidden" id="W3ResultsTableHidden" runat="server">
                        <asp:Table ID="w3Table" CssClass="table table-hover" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-3" />
                                <asp:TableHeaderCell Scope="Column" Text="Type" CssClass="col-md-1" />
                                <asp:TableHeaderCell Scope="Column" Text="Regel" CssClass="col-md-1" />
                                <asp:TableHeaderCell Scope="Column" Text="Kolom" CssClass="col-md-1" />
                                <asp:TableHeaderCell Scope="Column" Text="Melding" CssClass="col-md-6" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>

