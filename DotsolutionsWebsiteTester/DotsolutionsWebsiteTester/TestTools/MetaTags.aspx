<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MetaTags.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.MetaTags" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="MetaTags">
            <div class="panel panel-custom" id="MetaTagsSession" runat="server">
                <div class="panel-heading">
                    <span id="MetaTagsRating" runat="server">?</span><span class="title">Meta tags</span>
                </div>
                <div class="panel-body">
                    <div id="MetaErrorsFound" class="results" runat="server"></div>
                    <div class="hidden" id="MetaResultsTableHidden" runat="server">
                        <asp:Table ID="MetaResultsTable" CssClass="table table-hover word-break" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="#C7E5F4" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-3" />
                                <asp:TableHeaderCell Scope="Column" Text="Type" CssClass="col-md-2" />
                                <asp:TableHeaderCell Scope="Column" Text="Name" CssClass="col-md-2" />
                                <asp:TableHeaderCell Scope="Column" Text="Content" CssClass="col-md-5" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
