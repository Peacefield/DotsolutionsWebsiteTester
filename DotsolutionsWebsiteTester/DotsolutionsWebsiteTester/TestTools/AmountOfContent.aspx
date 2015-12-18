<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AmountOfContent.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.AmountOfContent" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="AmountOfContent">
            <div class="panel panel-custom" id="AmountOfContentSession" runat="server">
                <div class="panel-heading">
                    <span id="AmountOfContentRating" runat="server">?</span><span class="title">Hoeveelheid Content</span>
                </div>
                <div class="panel-body">
                    <div id="AmountOfContentResults" class="results" runat="server"></div>
                    <div class="hidden" id="AmountOfContentTableHidden" runat="server">
                        <asp:Table ID="AmountOfContentTable" CssClass="table table-hover" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="" CssClass="col-md-1" />
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-8" />
                                <asp:TableHeaderCell Scope="Column" Text="Aantal woorden" CssClass="col-md-3" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>

