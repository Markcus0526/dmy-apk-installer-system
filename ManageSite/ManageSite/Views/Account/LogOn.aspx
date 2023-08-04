<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<ManageSite.Models.LogOnModel>" %>

<!DOCTYPE html>
<!--[if IE 8]> <html lang="en" class="ie8"> <![endif]-->
<!--[if IE 9]> <html lang="en" class="ie9"> <![endif]-->
<!--[if !IE]><!--> <html lang="en"> <!--<![endif]-->

<head id="Head1" runat="server">
	<meta charset="utf-8" />
	<title>安卓软件批量安装系统</title>
	<meta content="width=device-width, initial-scale=1.0" name="viewport" />
	<meta content="" name="description" />
	<meta content="" name="author" />
	<!-- BEGIN GLOBAL MANDATORY STYLES -->
	<link href="/Content/plugins/bootstrap/css/bootstrap.min.css" rel="stylesheet" type="text/css"/>
	<link href="/Content/plugins/bootstrap/css/bootstrap-responsive.min.css" rel="stylesheet" type="text/css"/>
	<link href="/Content/plugins/font-awesome/css/font-awesome.min.css" rel="stylesheet" type="text/css"/>
	<link href="/Content/css/style-metro.css" rel="stylesheet" type="text/css"/>
	<link href="/Content/css/style.css" rel="stylesheet" type="text/css"/>
	<link href="/Content/css/style-responsive.css" rel="stylesheet" type="text/css"/>
	<link href="/Content/css/themes/default.css" rel="stylesheet" type="text/css" id="style_color"/>
	<link href="/Content/plugins/uniform/css/uniform.default.css" rel="stylesheet" type="text/css"/>
	<link rel="Stylesheet" type="text/css" href="/Content/plugins/select2/select2_metro.css" />
	<!-- END GLOBAL MANDATORY STYLES -->
	<!-- BEGIN PAGE LEVEL STYLES -->
	<link href="/Content/css/pages/login-soft.css" rel="stylesheet" type="text/css"/>
	<!-- END PAGE LEVEL STYLES -->
	<link rel="shortcut icon" href="favicon.ico" />
</head>

<!-- BEGIN BODY -->
<body class="login">
	<!-- BEGIN LOGO -->
	<div class="logo">
		<img src="/Content/img/logo-big.png" alt="" /> 
	</div>
	<!-- END LOGO -->
	<!-- BEGIN LOGIN -->
	<div class="content">
		<!-- BEGIN LOGIN FORM -->
        <% using (Html.BeginForm("LogOn", "Account", FormMethod.Post, new { @class = "form-vertical login-form" }))
           { %>
			<h3 class="form-title" style="text-align:center;">登录账户</h3>
            <% if (!ViewData.ModelState.IsValid) { %>
			<div class="alert alert-error">
				<button class="close" data-dismiss="alert"></button>
				<span><%= Html.ValidationMessage("modelerror") %></span>
			</div>
            <% } %>
			<div class="control-group">
				<!--ie8, ie9 does not support html5 placeholder, so we just show field title for that-->
				<label class="control-label visible-ie8 visible-ie9">帐号：</label>
				<div class="controls">
					<div class="input-icon left">
						<i class="icon-user"></i>
						<input class="m-wrap placeholder-no-fix" type="text" autocomplete="off" placeholder="帐号" name="username"/>
					</div>
				</div>
			</div>
			<div class="control-group">
				<label class="control-label visible-ie8 visible-ie9">密码：</label>
				<div class="controls">
					<div class="input-icon left">
						<i class="icon-lock"></i>
						<input class="m-wrap placeholder-no-fix" type="password" autocomplete="off" placeholder="请输入您的密码" name="password"/>
					</div>
				</div>
			</div>
			<div class="form-actions">
				<label class="checkbox">
				<input type="checkbox" name="remember" value="1"/> 自动登录
				</label>
				<button type="submit" class="btn blue pull-right">
				立即登录 <i class="m-icon-swapright m-icon-white"></i>
				</button>            
			</div>
        <% } %>   
		<!-- END LOGIN FORM -->        
	</div>
	<!-- END LOGIN -->
	<!-- BEGIN COPYRIGHT -->
	<div class="copyright">
		2011-2013 &copy; 德铭源科技有限公司
	</div>
	<!-- END COPYRIGHT -->
	<!-- BEGIN JAVASCRIPTS(Load javascripts at bottom, this will reduce page load time) -->
	<!-- BEGIN CORE PLUGINS -->   <script src="/Content/plugins/jquery-1.10.1.min.js" type="text/javascript"></script>
	<script src="/Content/plugins/jquery-migrate-1.2.1.min.js" type="text/javascript"></script>
	<!-- IMPORTANT! Load jquery-ui-1.10.1.custom.min.js before bootstrap.min.js to fix bootstrap tooltip conflict with jquery ui tooltip -->
	<script src="/Content/plugins/jquery-ui/jquery-ui-1.10.1.custom.min.js" type="text/javascript"></script>      
	<script src="/Content/plugins/bootstrap/js/bootstrap.min.js" type="text/javascript"></script>
	<script src="/Content/plugins/bootstrap-hover-dropdown/twitter-bootstrap-hover-dropdown.min.js" type="text/javascript" ></script>
	<!--[if lt IE 9]>
	<script src="/Content/plugins/excanvas.min.js"></script>
	<script src="/Content/plugins/respond.min.js"></script>  
	<![endif]-->   
	<script src="/Content/plugins/jquery-slimscroll/jquery.slimscroll.min.js" type="text/javascript"></script>
	<script src="/Content/plugins/jquery.blockui.min.js" type="text/javascript"></script>  
	<script src="/Content/plugins/jquery.cookie.min.js" type="text/javascript"></script>
	<script src="/Content/plugins/uniform/jquery.uniform.min.js" type="text/javascript" ></script>
	<!-- END CORE PLUGINS -->
	<!-- BEGIN PAGE LEVEL PLUGINS -->
	<script src="/Content/plugins/jquery-validation/dist/jquery.validate.min.js" type="text/javascript"></script>
	<script src="/Content/plugins/backstretch/jquery.backstretch.min.js" type="text/javascript"></script>
	<script type="text/javascript" src="/Content/plugins/select2/select2.min.js"></script>
	<!-- END PAGE LEVEL PLUGINS -->
	<!-- BEGIN PAGE LEVEL SCRIPTS -->
	<script src="/Content/scripts/app.js" type="text/javascript"></script>
	<script src="/Content/scripts/login-soft.js" type="text/javascript"></script>      
	<!-- END PAGE LEVEL SCRIPTS --> 
	<script>
	    jQuery(document).ready(function () {
	        App.init();
	        Login.init();
	    });
	</script>
	<!-- END JAVASCRIPTS -->
<script type="text/javascript">    var _gaq = _gaq || []; _gaq.push(['_setAccount', 'UA-37564768-1']); _gaq.push(['_setDomainName', 'keenthemes.com']); _gaq.push(['_setAllowLinker', true]); _gaq.push(['_trackPageview']); (function () { var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true; ga.src = ('https:' == document.location.protocol ? 'https://' : 'http://') + 'stats.g.doubleclick.net/dc.js'; var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s); })();</script></body>
<!-- END BODY -->
</html>
