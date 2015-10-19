<%@ Page Title="Handmatige Test" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Handmatige-Test.aspx.cs" Inherits="DotsolutionsWebsiteTester.HandmatigeTest" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h1>Handmatige Test</h1>
        <asp:Button ID="SkipTestBtn"
                    Text="Sla over"
                    OnClick="SkipTest_Click"
                    CssClass="btn btn-success btn-md"
                    runat="server" />
        <article>
            <h2>Vormgeving</h2>
            <h3>Professionaliteit</h3>
            <h3>Gebruiksvriendelijkheid</h3>
            <h3>Opmerkingen</h3>
        </article>
    </div>
</asp:Content>
