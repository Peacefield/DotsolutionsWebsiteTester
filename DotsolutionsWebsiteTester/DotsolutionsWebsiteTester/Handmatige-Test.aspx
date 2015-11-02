<%@ Page Title="Handmatige Test" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Handmatige-Test.aspx.cs" Inherits="DotsolutionsWebsiteTester.HandmatigeTest" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="jumbotron">
        <h1>Handmatige Test</h1>

        <span class="help-block" id="SiteUrl" runat="server">http://www.example.com</span>
        <div class="hidden" id="required" runat="server">
            <div class="alert alert-danger" role="alert">
                <span class="glyphicon glyphicon-alert" aria-hidden="true"></span>
                <span class="sr-only">Error:</span> Niet alle verplichte velden zijn correct ingevuld.
            </div>
        </div>

        <article style="width: 100%">
            <h2>Vormgeving</h2>
            Hoe is de vormgeving van de site? Is de site duidelijk en overzichtelijk en kan een bezoeker eenvoudig zijn weg vinden binnen de site?
            <article>
                <h3>Professionaliteit*</h3>
                <label class="control-label col-md-2">Opmaak</label>
                <div class="col-md-10">
                    <asp:TextBox ID="VormProfOpma"
                        runat="server"
                        CssClass="stars"
                        data-max="10"
                        type="number" />
                    <span class="help-block">Is de opmaak netjes en verzorgd?</span>
                </div>
                <label class="control-label col-md-2">Huisstijl</label>
                <div class="col-md-10">
                    <asp:TextBox ID="VormProfHuis"
                        runat="server"
                        CssClass="stars"
                        data-max="10"
                        type="number" />
                    <span class="help-block">Sluit het aan bij de huisstijl van het bedrijf?</span>
                </div>

                <label class="control-label col-md-2">Kleurgebruik</label>
                <div class="col-md-10">
                    <asp:TextBox ID="VormProfKleur"
                        runat="server"
                        CssClass="stars"
                        data-max="10"
                        type="number" />
                    <span class="help-block">Is het kleurgebruik netjes en professioneel?</span>
                </div>
            </article>

            <article>
                <h3>Gebruiksvriendelijkheid*</h3>

                <label class="control-label col-md-2">Menu</label>
                <div class="col-md-10">
                    <asp:TextBox ID="VormUxMen"
                        runat="server"
                        CssClass="stars"
                        data-max="10"
                        type="number" />
                    <span class="help-block">Is het menu duidelijk?</span>
                </div>

                <label class="control-label col-md-2">Structuur</label>
                <div class="col-md-10">
                    <asp:TextBox ID="VormUxStruc"
                        runat="server"
                        CssClass="stars"
                        data-max="10"
                        type="number" />
                    <span class="help-block">Is de structuur van de site duidelijk voor een bezoeker?</span>
                </div>
            </article>

            <article>
                <h3>Opmerkingen</h3>
                <span class="help-block">Eventuele opmerkingen over de vormgeving.</span>
                <asp:TextBox ID="VormgevingOpmerking"
                    TextMode="MultiLine"
                    Rows="4"
                    CssClass="fullTextArea"
                    runat="server" />
            </article>
        </article>

        <article id="buttons">
            <asp:LinkButton ID="StartTestBtn"
                OnClick="StartTest_Click"
                CssClass="btn btn-success btn-md"
                runat="server">
            <span aria-hidden="true" class="glyphicon glyphicon-ok"></span>
            Ga verder
            </asp:LinkButton>
            <asp:LinkButton ID="SkipTestBtn"
                OnClick="SkipTest_Click"
                CssClass=""
                runat="server">
            <span aria-hidden="true" class="glyphicon glyphicon-remove"></span>
            Overslaan
            </asp:LinkButton>
        </article>
        <span class="help-block">* Verplichte velden</span>
    </div>
    <script src="Scripts/Custom/bootstrap-rating-input.js"></script>
</asp:Content>
