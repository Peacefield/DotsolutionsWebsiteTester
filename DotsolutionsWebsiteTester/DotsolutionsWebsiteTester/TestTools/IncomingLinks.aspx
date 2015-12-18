<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="IncomingLinks.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.IncomingLinks" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="IncomingLinks">
            <div class="panel panel-custom" id="IncomingLinksSession" runat="server">
                <div class="panel-heading">
                    <span id="IncomingLinksRating" runat="server">?</span><span class="title">Binnenkomende links</span>
                </div>
                <div class="panel-body">
                    <div id="IncomingLinksResults" class="results" runat="server"></div>
                    <div class="hidden" id="IncomingLinksTableHidden" runat="server">
                        <asp:Table ID="IncomingLinksTable" CssClass="table table-hover" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-4" />
                                <asp:TableHeaderCell Scope="Column" Text="Aantal verwijzende links" CssClass="col-md-3" />
                                <asp:TableHeaderCell Scope="Column" Text="Mozrank Score" CssClass="col-md-2" />
                                <asp:TableHeaderCell Scope="Column" Text="Laatst gemeten" CssClass="col-md-3" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>