<%@ Page Language="C#" %>
<%@ Import Namespace="System.Threading" %>
<html>
<body>
<form runat=server>
<h1>User: <%= User.ToString() %></h1>
<h1>User.Identity: <%= User.Identity.ToString() %></h1>
<h1>User.Identity.Name: <%= User.Identity.Name %></h1>
<h1>User.Identity.IsAuthenticated: <%= User.Identity.IsAuthenticated.ToString() %></h1>
<h1>Thread.CurrentPrincipal: <%= Thread.CurrentPrincipal.ToString() %></h1>
<h1>Thread.CurrentPrincipal.Identity: <%=Thread.CurrentPrincipal.Identity.ToString() %></h1>
<h1>Thread.CurrentPrincipal.Identity.Name: <%=Thread.CurrentPrincipal.Identity.Name %></h1>
<h1>Thread.CurrentPrincipal.Identity.IsAuthenticated: <%=Thread.CurrentPrincipal.Identity.IsAuthenticated.ToString() %></h1>
</form>
</body>
</html>