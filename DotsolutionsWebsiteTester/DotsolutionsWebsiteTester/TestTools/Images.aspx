<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Images.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Images" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Images">
            <div class="panel panel-custom" id="ImagesSession" runat="server">
                <div class="panel-heading">
                    <span id="ImagesRating" runat="server">?</span> Afbeeldingen
                </div>
                <div class="panel-body">
                    <div id="ImagesMessages" class="results" runat="server"></div>
                    <div class="hidden" id="ImagesTableHidden" runat="server">
                        <asp:Table ID="table" CssClass="table table-hover word-break" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-3" />
                                <asp:TableHeaderCell Scope="Column" Text="Afbeelding" CssClass="col-md-3" />
                                <asp:TableHeaderCell Scope="Column" Text="Melding" CssClass="col-md-6" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>