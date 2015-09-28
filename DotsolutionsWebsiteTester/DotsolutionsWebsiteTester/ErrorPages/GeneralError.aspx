<%@ Page Title="Error" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GeneralError.aspx.cs" Inherits="DotsolutionsWebsiteTester.ErrorPages.GeneralError" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron text-center center-block">
        <h1>Er is iets misgegaan</h1>
        <pre>
            Error Message:<br />
            <asp:Label ID="exMessage" runat="server" Font-Bold="true"
                Font-Size="Large" />
        </pre>
    </div>

</asp:Content>
