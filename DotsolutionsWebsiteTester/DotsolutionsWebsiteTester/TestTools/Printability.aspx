<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Printability.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Printability" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Printability">
            <div class="panel panel-primary" id="PrintabilitySession" runat="server">
                <div class="panel-heading">Printbaarheid</div>
                <div class="panel-body">
                    <div id="PrintResults" runat="server"></div>
                    <div class="hidden" id="PrintabilityTableHidden" runat="server">
                        <asp:Table ID="PrintabilityTable" CssClass="table table-hover" runat="server">
                            <asp:TableHeaderRow BackColor="LightBlue" runat="server">
                                <asp:TableHeaderCell Scope="Column" Text="Melding" CssClass="col-md-6"/>
                                <asp:TableHeaderCell Scope="Column" Text="Pagina" CssClass="col-md-3"/>
                                <asp:TableHeaderCell Scope="Column" Text="CSS" CssClass="col-md-3"/>
                            </asp:TableHeaderRow>
                        </asp:Table>
                    </div>  
                </div>
            </div>
        </div>

    </div>
</asp:Content>