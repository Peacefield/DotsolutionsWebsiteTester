<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="InternalLinks.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.InternalLinks" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="InternalLinks">
            <div class="panel panel-primary">
                <div class="panel-heading">Interne links</div>

                <div id="internalLinksErrorsFound" runat="server"></div>
                <div class="panel-body">
                    <div class="hidden" id="IntLinksHiddenTable" runat="server">
                        <asp:Table ID="IntLinksTable" CssClass="table table-hover" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow1" BackColor="LightBlue" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Link van URL" />
                                <asp:TableHeaderCell Scope="Column" Text="Melding" />
                                <asp:TableHeaderCell Scope="Column" Text="Op pagina" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>