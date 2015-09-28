<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="CodeQuallity.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.CodeQuallity" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">
        
        <div id="CodeQuality">
            <div class="panel panel-primary">
                <div class="panel-heading">Code quality</div>
                <div id="w3ErrorsFound" runat="server"></div>
                <div class="panel-body">
                    <div class="hidden" id="W3ResultsTableHidden" runat="server">
                        <asp:Table ID="table" CssClass="table table-hover" runat="server">
                            <asp:TableHeaderRow ID="TableHeaderRow" BackColor="LightBlue" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" />
                                <asp:TableHeaderCell Scope="Column" Text="Type" />
                                <asp:TableHeaderCell Scope="Column" Text="Regel" />
                                <asp:TableHeaderCell Scope="Column" Text="Kolom" />
                                <asp:TableHeaderCell Scope="Column" Text="Melding" />
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>

