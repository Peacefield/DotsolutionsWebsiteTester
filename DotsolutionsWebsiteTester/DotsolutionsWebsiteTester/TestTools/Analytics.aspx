<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Analytics.aspx.cs" Inherits="DotsolutionsWebsiteTester.TestTools.Analytics" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div id="result">

        <div id="Analytics">
            <div class="panel panel-custom" id="AnalyticsSession" runat="server">
                <div class="panel-heading">
                    <span id="Rating" runat="server">?</span>Analytics
                </div>
                <div class="panel-body">
                    <div id="AnalyticsResults" class="results" runat="server"></div>
                    <div class="hidden" id="AnalyticsTableHidden" runat="server">
                        <div class="table-responsive">
                            <asp:Table ID="AnalyticsTable" CssClass="table table-hover" runat="server">
                                <asp:TableHeaderRow ID="TableHeaderRow2" BackColor="#C7E5F4" runat="server">
                                    <asp:TableHeaderCell Scope="Column" Text="Type analytics software" CssClass="col-md-10" />
                                    <asp:TableHeaderCell Scope="Column" Text="Percentage gevonden" CssClass="col-md-2" />
                                </asp:TableHeaderRow>
                            </asp:Table>
                        </div>
                    </div>
                </div>
            </div>
        </div>

    </div>
</asp:Content>

