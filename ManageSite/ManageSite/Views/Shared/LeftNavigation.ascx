<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<ul class="page-sidebar-menu">
	<li>
		<!-- BEGIN SIDEBAR TOGGLER BUTTON -->
		<div class="sidebar-toggler hidden-phone"></div>
		<!-- BEGIN SIDEBAR TOGGLER BUTTON -->
	</li>
    <li class="start">
		<a href="<%= ViewData["rootUri"] %>">
		<i class="icon-home"></i> 
		<span class="title">首页</span>
		</a>
    </li>
    <%=Html.MvcSiteMap().LeftNavigation(SiteMap.RootNode, "active")%>
</ul>
