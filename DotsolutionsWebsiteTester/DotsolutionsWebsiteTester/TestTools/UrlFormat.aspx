<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="UrlFormat.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.UrlFormat" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="UrlFormat">
            <div class="panel panel-custom" id="UrlFormatSession" runat="server">
                <div class="panel-heading">
                    <span id="UrlFormatRating" runat="server">?</span>URL Formaat</div>
                <div class="panel-body">
                    <div id="UrlFormatNotifications" runat="server"></div>
                    <div class="hidden" id="UrlFormatHiddenTable" runat="server">
                        <asp:Table ID="UrlFormatTable" CssClass="table table-hover word-break" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow1" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Link van URL" CssClass="col-md-6" />
                                <asp:TableHeaderCell Scope="Column" Text="Melding" CssClass="col-md-3" />
                                <asp:TableHeaderCell Scope="Column" Text="Op pagina" CssClass="col-md-3" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>