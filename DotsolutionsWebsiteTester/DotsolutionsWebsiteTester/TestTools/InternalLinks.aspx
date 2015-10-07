<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="InternalLinks.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.InternalLinks" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="InternalLinks">
            <div class="panel panel-custom" id="InternalLinksSession" runat="server">
                <div class="panel-heading">Interne links</div>

                <div class="panel-body">
                    <div id="internalLinksErrorsFound" runat="server"></div>
                    <div class="hidden" id="IntLinksHiddenTable" runat="server">
                        <div class="table-responsive">
                            <asp:Table ID="IntLinksTable" CssClass="table table-hover" runat="server">
                                <asp:TableHeaderRow ID="TableHeaderRow1" BackColor="#C7E5F4" runat="server">
                                    <asp:TableHeaderCell Scope="Column" Text="Link van URL" CssClass="col-md-6" />
                                    <asp:TableHeaderCell Scope="Column" Text="Melding" CssClass="col-md-5" />
                                    <asp:TableHeaderCell Scope="Column" Text="Op pagina" CssClass="col-md-1" />
                                </asp:TableHeaderRow>
                            </asp:Table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>
